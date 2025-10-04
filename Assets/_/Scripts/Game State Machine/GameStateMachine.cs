namespace Prototype
{
    public abstract class GameStateMachine : AnimatorStateBehaviour
    {
        SoundEffectInstance music;

        protected void ChangeMusic(SoundEffectCollection music, float volume)
        {
            if (!this.music && !music)
                return;

            if (this.music && music && this.music.clip == music.GetClip())
                return;

            if (this.music)
                this.music.Destroy();

            this.music = music ? music.PlayClip(new()
            {
                volume = volume,
                loop = true,
                fade = new(2, 2)
            }) : null;
        }
    }
}
