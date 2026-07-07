using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class HorizontalCardOverlapper : MonoBehaviour
{
    RectTransform rectTransform;
    HorizontalLayoutGroup horizontalLayoutGroup;
    private void Awake()
    {
        rectTransform =GetComponent<RectTransform>();
        horizontalLayoutGroup= GetComponent<HorizontalLayoutGroup>();
    }

    // Update is called once per frame
    private void OnTransformChildrenChanged()
    {
        float sum =0;
        foreach (RectTransform child in rectTransform)
        {
            sum += child.rect.width;
        }
        var extra =rectTransform.rect.width-sum;
        extra/=transform.childCount;
        
        horizontalLayoutGroup.spacing=extra;    
    }
}
