using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FtueTransition : MonoBehaviour
{
    private Button btn;
    [SerializeField] private Canvas hide;
    [SerializeField] private Canvas showup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnButtonClick()
    {
        if (showup != null) showup.gameObject.SetActive(true);
        if (hide != null) hide.gameObject.SetActive(false);
    }
}
