using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    private void Awake()
    {
        instance = this;
    }

    public AudioSource[] deathSounds;
    public AudioSource[] laughSounds;
    public AudioSource[] takeDamageSounds;
    public AudioSource[] dealDamageSounds;
    public AudioSource[] dealRangeDamageSounds;

    public void PlayDeath()
    {
        deathSounds[Random.Range(0, deathSounds.Length)].Play();
    }
    public void PlayLaugh()
    {
        laughSounds[Random.Range(0, laughSounds.Length)].Play();
    }
    public void PlayTakeDamage()
    {
        takeDamageSounds[Random.Range(0, takeDamageSounds.Length)].Play();
    }
    public void PlayDealDamage()
    {
        dealDamageSounds[Random.Range(0, dealDamageSounds.Length)].Play();
    }
    public void PlayDealRangedDamage()
    {
        dealRangeDamageSounds[Random.Range(0, dealRangeDamageSounds.Length)].Play();
    }
}
