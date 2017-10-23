using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingCountScript : MonoBehaviour
{

    public CityObserver CityObserver;

    public int TypeId;

    public string Prefix = "Count";

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
		// Has the city data changed?
        if (this.CityObserver.Fresh)
        {
			// Count up the buildings with this.typeid
            var counter = 0;
            foreach (var b in this.CityObserver.LastPacket.predict.grid)
            {
                if (b.type == this.TypeId) counter += 1;
            }

			// Set the text
            this.gameObject.GetComponent<Text>().text = Prefix + " " + counter;
        }
    }
}
