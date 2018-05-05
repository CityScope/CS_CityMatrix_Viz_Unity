using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI
{
    /// <summary>Change the Focus on from a Window.</summary>
    public class UIFocus : MonoBehaviour
    {
        public string CanvasName = "Canvas";

        private UIWindowManager manager;
        private Image image;

        private Transform tf;

        public void Start()
        {
            manager = GameObject.Find(CanvasName).GetComponent<UIWindowManager>();

            image = transform.Find("Panel/Header").GetComponent<Image>();

            tf = transform;
        }

        public void onPanelEnter()
        {
            manager.ChangeState(gameObject);

            Color c = image.color;
            c.a = 255;
            image.color = c;

            tf.SetAsLastSibling(); //move to the front (on parent)
            tf.SetAsFirstSibling(); //move to the back (on parent)
            tf.SetSiblingIndex(-1); //move to position, whereas 0 is the backmost, transform.parent.childCount -1 is the frontmost position 
            tf.GetSiblingIndex(); //get the position in the hierarchy (on parent)
        }
    }
}
// © 2017 crosstales LLC (https://www.crosstales.com)