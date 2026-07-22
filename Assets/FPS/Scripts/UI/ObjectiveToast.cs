using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class ObjectiveToast : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Text content that will display the title")]
        public TMPro.TextMeshProUGUI TitleTextContent;

        [Tooltip("Text content that will display the description")]
        public TMPro.TextMeshProUGUI DescriptionTextContent;

        [Tooltip("Text content that will display the counter")]
        public TMPro.TextMeshProUGUI CounterTextContent;

        [Tooltip("Rect that will display the description")]
        public RectTransform SubTitleRect;

        [Tooltip("Canvas used to fade in and out the content")]
        public CanvasGroup CanvasGroup;

        [Tooltip("Layout group containing the objective")]
        public HorizontalOrVerticalLayoutGroup LayoutGroup;


        [Header("Transitions")]
        [Tooltip("Delay before moving complete")]
        public float CompletionDelay;

        [Tooltip("Duration of the fade in")]
        public float FadeInDuration = 0.5f;

        [Tooltip("Duration of the fade out")]
        public float FadeOutDuration = 2f;


        [Header("Sound")]
        [Tooltip("Sound that will be played on initialization")]
        public AudioClip InitSound;

        [Tooltip("Sound that will be played on completion")]
        public AudioClip CompletedSound;

        [Tooltip("Volume applied to all objective toast sounds")]
        [Range(0f, 1f)]
        public float SoundVolume = 1f;

        [Tooltip("Pitch applied to all objective toast sounds")]
        [Range(0.1f, 3f)]
        public float SoundPitch = 1f;


        [Header("Movement")]
        [Tooltip("Time it takes to move in the screen")]
        public float MoveInDuration = 0.5f;

        [Tooltip("Animation curve for move in, position in x over time")]
        public AnimationCurve MoveInCurve;

        [Tooltip("Time it takes to move out of the screen")]
        public float MoveOutDuration = 2f;

        [Tooltip("Animation curve for move out, position in x over time")]
        public AnimationCurve MoveOutCurve;


        float m_StartFadeTime;
        bool m_IsFadingIn;
        bool m_IsFadingOut;
        bool m_IsMovingIn;
        bool m_IsMovingOut;

        AudioSource m_AudioSource;
        RectTransform m_RectTransform;


        public void Initialize(
            string titleText,
            string descText,
            string counterText,
            bool isOptionnal,
            float delay)
        {
            Canvas.ForceUpdateCanvases();

            TitleTextContent.text = titleText;
            DescriptionTextContent.text = descText;
            CounterTextContent.text = counterText;

            if (GetComponent<RectTransform>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    GetComponent<RectTransform>());
            }

            m_StartFadeTime = Time.time + delay;

            m_IsFadingIn = true;
            m_IsMovingIn = true;
        }


        public void Complete()
        {
            m_StartFadeTime = Time.time + CompletionDelay;

            m_IsFadingIn = false;
            m_IsMovingIn = false;

            // Play completion sound
            PlaySound(CompletedSound);

            m_IsFadingOut = true;
            m_IsMovingOut = true;
        }


        void Update()
        {
            float timeSinceFadeStarted =
                Time.time - m_StartFadeTime;

            SubTitleRect.gameObject.SetActive(
                !string.IsNullOrEmpty(
                    DescriptionTextContent.text
                )
            );


            // FADE IN
            if (m_IsFadingIn && !m_IsFadingOut)
            {
                if (timeSinceFadeStarted < FadeInDuration)
                {
                    CanvasGroup.alpha =
                        timeSinceFadeStarted / FadeInDuration;
                }
                else
                {
                    CanvasGroup.alpha = 1f;

                    m_IsFadingIn = false;

                    // Play initialization sound
                    PlaySound(InitSound);
                }
            }


            // MOVE IN
            if (m_IsMovingIn && !m_IsMovingOut)
            {
                if (timeSinceFadeStarted < MoveInDuration)
                {
                    LayoutGroup.padding.left =
                        (int)MoveInCurve.Evaluate(
                            timeSinceFadeStarted / MoveInDuration
                        );

                    if (GetComponent<RectTransform>())
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(
                            GetComponent<RectTransform>()
                        );
                    }
                }
                else
                {
                    LayoutGroup.padding.left = 0;

                    if (GetComponent<RectTransform>())
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(
                            GetComponent<RectTransform>()
                        );
                    }

                    m_IsMovingIn = false;
                }
            }


            // FADE OUT
            if (m_IsFadingOut)
            {
                if (timeSinceFadeStarted < FadeOutDuration)
                {
                    CanvasGroup.alpha =
                        1 -
                        (timeSinceFadeStarted / FadeOutDuration);
                }
                else
                {
                    CanvasGroup.alpha = 0f;

                    m_IsFadingOut = false;

                    Destroy(gameObject);
                }
            }


            // MOVE OUT
            if (m_IsMovingOut)
            {
                if (timeSinceFadeStarted < MoveOutDuration)
                {
                    LayoutGroup.padding.left =
                        (int)MoveOutCurve.Evaluate(
                            timeSinceFadeStarted / MoveOutDuration
                        );

                    if (GetComponent<RectTransform>())
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(
                            GetComponent<RectTransform>()
                        );
                    }
                }
                else
                {
                    m_IsMovingOut = false;
                }
            }
        }


        void PlaySound(AudioClip sound)
        {
            if (!sound)
                return;

            if (!m_AudioSource)
            {
                m_AudioSource =
                    gameObject.AddComponent<AudioSource>();

                m_AudioSource.outputAudioMixerGroup =
                    AudioUtility.GetAudioGroup(
                        AudioUtility.AudioGroups.HUDObjective
                    );

                // Keep AudioSource volume at maximum.
                // SoundVolume controls the actual playback volume.
                m_AudioSource.volume = 1f;

                // UI sounds are 2D.
                m_AudioSource.spatialBlend = 0f;
            }

            // Apply the configured pitch.
            m_AudioSource.pitch = SoundPitch;

            // Play at the configured volume.
            m_AudioSource.PlayOneShot(
                sound,
                SoundVolume
            );
        }
    }
}