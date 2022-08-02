using System;
using UnityEngine;
using UnityEngine.UI;

namespace DecentM.Prefabs.UI
{
    public class Dropdown : MonoBehaviour
    {
        public GameObject optionTemplate;
        public Transform optionsRoot;
        public Animator animator;
        public Button button;

        private MonoBehaviour listener;
        private string onChangeEventName;

        private GameObject[] instantiatedOptions;

        private void Start()
        {
            this.optionTemplate.SetActive(false);
            this.Clear();
        }

        public void SetListener(MonoBehaviour listener, string onChangeEventName)
        {
            this.listener = listener;
            this.onChangeEventName = onChangeEventName;
        }

        public void Clear()
        {
            if (this.instantiatedOptions == null)
                this.instantiatedOptions = new GameObject[0];

            foreach (GameObject option in this.instantiatedOptions)
            {
                Destroy(option);
            }

            this.options = new object[0][];
            this.instantiatedOptions = new GameObject[0];
        }

        private float elapsed = 0;
        private float instantinatingDelay = 0.15f;

        private object[][] options;

        private void FixedUpdate()
        {
            if (this.options.Length == this.instantiatedOptions.Length)
                return;

            this.elapsed += Time.fixedUnscaledDeltaTime;
            if (elapsed <= this.instantinatingDelay)
                return;
            this.elapsed = 0;

            int newIndex = this.instantiatedOptions.Length;
            if (newIndex >= this.options.Length || this.options[newIndex] == null)
                return;

            this.AddOption(this.options[newIndex]);
        }

        private void AddOption(object[] optionKvp)
        {
            if (this.instantiatedOptions == null)
                this.instantiatedOptions = new GameObject[0];

            GameObject instance = Instantiate(this.optionTemplate);

            instance.transform.SetParent(this.optionsRoot);
            instance.name = $"{optionKvp[0]}_{optionKvp[1]}";
            instance.transform.SetPositionAndRotation(
                this.optionTemplate.transform.position,
                this.optionTemplate.transform.rotation
            );
            instance.transform.localScale = this.optionTemplate.transform.localScale;

            DropdownOption option = instance.GetComponent<DropdownOption>();
            if (option == null)
                return;

            option.SetData(this, optionKvp[0], (string)optionKvp[1]);
            instance.SetActive(true);

            GameObject[] tmp = new GameObject[this.instantiatedOptions.Length + 1];
            Array.Copy(this.instantiatedOptions, tmp, this.instantiatedOptions.Length);
            tmp[tmp.Length - 1] = instance;
            this.instantiatedOptions = tmp;
        }

        public void SetOptions(string[][] options)
        {
            this.Clear();
            this.options = new object[options.Length][];

            for (int i = 0; i < options.Length; i++)
            {
                this.options[i] = new object[] { options[i][0], options[i][1] };
            }
        }

        private object value = null;

        public object GetValue()
        {
            return this.value;
        }

        public void OnValueClick(object value)
        {
            this.value = value;
            this.listener.Invoke(this.onChangeEventName, 0);
            this.animator.SetBool("DropdownOpen", false);
        }

        public void OnButtonClick()
        {
            this.animator.SetBool("DropdownOpen", !this.animator.GetBool("DropdownOpen"));
        }
    }
}
