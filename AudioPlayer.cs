using Plugin.Maui.Audio;
//Basic Audio Player
namespace Project2A
{
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
            await Task.Delay(100);
        }

    }
}