using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MeteorlogAggregator
{
    public class AudioTester
    {
        public AudioTester(string audioDeviceName)
        {
            this.audioDeviceName = audioDeviceName;
            this.lockObject = new();
            this.isFlushing = true;
        }

        private string audioDeviceName;
        private readonly object lockObject;
        private bool isFlushing;
        private float maxDetectedLevel;

        /// <summary>
        /// Check the Audio device if there is any signal on it.
        /// </summary>
        /// <returns></returns>
        public bool IsAudioSignal()
        {
            int? deviceIndex = this.FindInputDeviceIndex();

            if (deviceIndex == null)
            {
                throw new Exception($"Nem található '{this.audioDeviceName}' nevű audio bemeneti eszköz.");
            }

            Console.WriteLine($"'{audioDeviceName}' megtalálva! Ellenőrzés indul...");

            using (var waveIn = new WaveInEvent())
            {
                waveIn.DeviceNumber = deviceIndex.Value;
                waveIn.WaveFormat = new WaveFormat(44100, 1); // 44.1kHz, mono
                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.StartRecording();

                Thread.Sleep(500);

                lock (lockObject)
                {
                    isFlushing = false;
                }

                Thread.Sleep(500);

                waveIn.StopRecording();
            }

            lock (lockObject)
            {
                Console.WriteLine($"Max level: {maxDetectedLevel}");
                return maxDetectedLevel > 0.01f;
            }
        }

        /// <summary>
        /// It returns the input device index if found. Null else.
        /// </summary>
        /// <returns></returns>
        private int? FindInputDeviceIndex()
        {
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var capabilities = WaveInEvent.GetCapabilities(i);
                if (capabilities.ProductName.Contains(this.audioDeviceName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return null;
        }

        /// <summary>
        /// Process the data on input device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short)(e.Buffer[i] | (e.Buffer[i + 1] << 8)); // 16 bites PCM
                float sampleValue = Math.Abs(sample / 32768f);

                lock (lockObject)
                {
                    if (!isFlushing)
                    {
                        if (sampleValue > this.maxDetectedLevel)
                            this.maxDetectedLevel = sampleValue;
                    }
                }
            }
        }
    }
}
