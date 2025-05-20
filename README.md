# MeteorLogAggregator
Small project for aggregate Spectumlab Meteorlog-YYYYMM.dat to RMOB-YYYYMM.dat file for Colorgamme Lab.
It takes two arguments:
- Source directory: The application looks for MeteorLog file in this directory. It uses always the current date!
- Destination directory: The result will be saved to this directory as RMOB-YYYYMM.dat file.

Flags:
```
-h --- HELP menu
-o --- Enable to override RMOB-YYYYMM.dat, when the input path is the same as the output path. 
-i --- Inverse aggregation function: RMOB-YYYYMM.dat -> MeteorLog. It can be used to restore the data with some losses, like frequency, etc.
-t [sec] --- Thread mode for service use. The app will not stop, and do the aggregation periodically in the given [sec] parameter.
```

## JSON Config
- There is a JSON file for the other settings. It's calles "AppSettings.json"

```json
{
  "AppSettings": {
    "KeepMaxBackups": "9",					// Keep last 9 file as backup.
    "BackupFilePrefix": "RMOB-backup-",		// Name prefix for backup
    "UseDiscordBot": true,					// Send notifications to Discord
    "DiscordWebhook": "YOUR_WEBHOOK_URL",	// Webhook URL for Discord
    "MinFreq": 1150,						// Minimal frequency for detection (Hz)
    "MaxFreq": 1250,						// Maximal frequency for detection (Hz)
    "NoDetectionThresholdMinutes": 20,		// It will send a notification if there were no detections in the given hour after 20 minutes
    "UseAudioDetection": true,				// It will check the sound input if it has signal on it. It's for checking SdrUno crashes.
    "AudioDeviceName": "CABLE"				// Name of the Audio device to check
  }
}

```
