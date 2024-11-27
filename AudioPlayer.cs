using Microsoft.Maui.Controls.PlatformConfiguration;
using Plugin.Maui.Audio;

namespace Project2A {
    public partial class AudioPlayer
    {
        private IAudioPlayer player;
        
        public async Task CreateAudioPlayer(String filename)
        {
            var filePath = await FileSystem.OpenAppPackageFileAsync(filename);
            player = AudioManager.Current.CreatePlayer(filePath);
        }
        public async Task PlayAudio()
        {
            player.Play();
        }

    }
}