using log4net;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FleetAudit.Common.Implementations
{
    /// <summary>
    /// Keeps the Wialon access token per user, encrypted with DPAPI, in
    /// <c>%LocalAppData%\FleetAudit\wialon-token.dat</c>, so no admin rights are needed.
    /// </summary>
    /// <remarks>
    /// The MSI installs per machine and the application runs without elevation, so a standard user cannot write the
    /// <c>.config</c> next to the exe. The <c>Token</c> key of that file is still read as a fallback
    /// (see <see cref="LoadOrLegacy()"/>), so portable copies and tokens set by an administrator keep working, but it is never written.
    /// </remarks>
    public sealed class WialonTokenStore
    {
        /// <summary>The appSettings key that held the token before it moved to the per-user store.</summary>
        public const string LegacySettingName = "Token";

        // Not a secret: it only makes the blob unreadable for code that calls DPAPI without knowing this application.
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("FleetAudit.Wialon.Token");

        // Static, so the instance registered in the container and the ones created elsewhere never write the file at once.
        private static readonly object Sync = new object();

        private readonly ILog _logger = LogManager.GetLogger(typeof(WialonTokenStore));

        public WialonTokenStore()
            : this(DefaultFilePath)
        { }

        public WialonTokenStore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A file path is required.", nameof(filePath));

            FilePath = filePath;
        }

        public static string DefaultFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FleetAudit",
            "wialon-token.dat");

        public string FilePath { get; }

        /// <summary>
        /// The saved token, or <c>null</c> when there is none: no file, or one that cannot be decrypted
        /// (damaged, or written by another Windows user or machine). Never throws, the user just enters the token again.
        /// </summary>
        public string Load()
        {
            lock (Sync)
            {
                try
                {
                    if (!File.Exists(FilePath))
                        return null;

                    var protectedBytes = File.ReadAllBytes(FilePath);
                    var bytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
                    var token = Encoding.UTF8.GetString(bytes);

                    return string.IsNullOrWhiteSpace(token) ? null : token;
                }
                catch (Exception ex)
                {
                    _logger.Warn($"The saved Wialon token could not be read: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// The saved token; when there is none, the <c>Token</c> appSetting of the application <c>.config</c>
        /// (<c>null</c> when that key is missing too).
        /// </summary>
        public string LoadOrLegacy()
        {
            return LoadOrLegacy(ConfigurationManager.AppSettings[LegacySettingName]);
        }

        /// <param name="legacyToken">The value to fall back to when no token is saved.</param>
        public string LoadOrLegacy(string legacyToken)
        {
            var token = Load();
            return string.IsNullOrWhiteSpace(token) ? legacyToken : token;
        }

        /// <summary>Encrypts and saves the token, creating the folder if needed. A blank token removes the saved one.</summary>
        public void Save(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (string.IsNullOrWhiteSpace(token))
            {
                Clear();
                return;
            }

            lock (Sync)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                var protectedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(token), Entropy, DataProtectionScope.CurrentUser);

                // Write a temporary file first, so an interrupted write never leaves a half-written token.
                var temporaryPath = FilePath + ".tmp";
                File.WriteAllBytes(temporaryPath, protectedBytes);

                if (File.Exists(FilePath))
                    File.Replace(temporaryPath, FilePath, null);
                else
                    File.Move(temporaryPath, FilePath);
            }
        }

        /// <summary>Removes the saved token. The legacy <c>Token</c> appSetting is not touched.</summary>
        public void Clear()
        {
            lock (Sync)
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
        }
    }
}
