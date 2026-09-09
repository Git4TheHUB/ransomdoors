using NAudio.Wave;
using System.IO;

namespace rans0m
{
    public class SoundHandle
    {
        private readonly WaveOut _waveOut;
        private readonly WaveFileReader _reader;
        private bool _disposed;
        private bool _looping;

        public SoundHandle(WaveOut waveOut, WaveFileReader reader)
        {
            _waveOut = waveOut;
            _reader = reader;
            _waveOut.PlaybackStopped += OnPlaybackStopped;
        }

        public void Play() => _waveOut.Play();

        public void PlayLooping()
        {
            _looping = true;
            _waveOut.Play();
        }

        private void OnPlaybackStopped(object s, StoppedEventArgs e)
        {
            if (_looping && !_disposed)
            {
                _reader.Position = 0;
                _waveOut.Play();
                return;
            }
            DisposeOnce();
        }

        public void Stop()
        {
            _looping = false;
            try { _waveOut.Stop(); } catch { }
            DisposeOnce();
        }

        private void DisposeOnce()
        {
            if (_disposed) return;
            _disposed = true;
            _waveOut.Dispose();
            _reader.Dispose();
        }
    }

    // Should use SoundHandle now instead and delete this
    public class SoundHelper
    {
        /// <summary>
        /// Creates a playable sound handle from an audio stream.
        /// </summary>
        /// <param name="wavStream">Audio Stream</param>
        /// <returns>Returns a new SoundHandle initialized with the audio data from the stream</returns>
        public static SoundHandle Create(Stream wavStream)
        {
            WaveFileReader reader = new WaveFileReader(wavStream);
            WaveOut waveOut = new WaveOut();

            waveOut.Init(reader);

            return new SoundHandle(waveOut, reader);
        }

    }
}
