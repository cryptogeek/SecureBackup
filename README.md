# Download:
https://github.com/cryptogeek/SecureBackup/raw/master/SecureBackup%20Portable.zip

# Description:
SecureBackup is a program to create encrypted backups from local folders to SFTP servers.

# Features:
-Zero-Knowledge Encryption: all data is encrypted with AES-256 before being sent to the server and decryption is only done locally.  
-Deduplication between backup states: only new or changed files are sent to server.  
-Backup log: see what changes occured at what time. Very useful in case of ransomware attack because you can restore to the state just before the attack. Also useful when a file was deleted by accident.  
-Backup explorer: browse all backup states and restore all files or only selected files.  
-Smart restore: skip files already restored.  
-Auto-backup: backups can be scheduled to run at chosen interval in the background.  
-SFTP: data transfer is done securely and file transfers are resumable.  
-MultiLang: SecureBackup is available in english and french and it's easy to add new languages.  
-Resumable: backups can be interrupted at any moment and will resume where they stopped even if the system is shutdown abruptly.  

# Libraries:
7-Zip: AES-256 Encryption  
WinSCP: SFTP file transfers  

# Requirements:
Microsoft .NET Framework 4.5.2 or above

# Supported client OS: 
Windows Vista, 7, 8, 8.1, 10  
Windows server 2008, 2012, 2016

# Supported SFTP server: 
OpenSSH on Ubuntu Server  
Other SFTP servers

# Screenshots:
![SecureBackup](https://raw.githubusercontent.com/cryptogeek/SecureBackup/master/Screenshots/Main.PNG)
![SecureBackup](https://raw.githubusercontent.com/cryptogeek/SecureBackup/master/Screenshots/Backup.png)
![SecureBackup](https://raw.githubusercontent.com/cryptogeek/SecureBackup/master/Screenshots/Backup%20settings.PNG)
![SecureBackup](https://raw.githubusercontent.com/cryptogeek/SecureBackup/master/Screenshots/Restore%20backup.png)

# Changelog:
https://raw.githubusercontent.com/cryptogeek/SecureBackup/master/changelog.txt
