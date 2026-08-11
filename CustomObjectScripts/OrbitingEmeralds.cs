using System.Linq;

namespace FP2_Sonic_Mod.CustomObjectScripts
{
    internal class OrbitingEmeralds : MonoBehaviour
    {
        // The array of Chaos Emeralds and whether they're done with their orbits.
        private GameObject[] Emeralds = new GameObject[7];
        private bool[] EmeraldsDone = new bool[7];

        // Whether or not these Emeralds are being expelled from the player.
        public bool expel;

        void Start()
        {
            // Loop through and add each Emerald to the array, resetting the Done flags.
            for (int emeraldIndex = 0; emeraldIndex < Emeralds.Length; emeraldIndex++)
            {
                Emeralds[emeraldIndex] = transform.GetChild(emeraldIndex).gameObject;
                EmeraldsDone[emeraldIndex] = false;

                // If these Emeralds are being expelled from the player, then center them and give them an equal rotation.
                if (expel)
                {
                    Emeralds[emeraldIndex].transform.position = transform.position;
                    Emeralds[emeraldIndex].transform.rotation = Quaternion.Euler(0, 0, -51.43f * emeraldIndex);
                }
            }
        }

        void Update()
        {
            // Rotate the pivot object.
            transform.Rotate(0, 0, -22.5f * FPStage.deltaTime);

            // Handle bringing these Emeralds inwards to the player.
            if (!expel)
            {
                // Loop through and reset the rotation of each Emerald, moving it to the pivot point.
                for (int emeraldIndex = 0; emeraldIndex < Emeralds.Length; emeraldIndex++)
                {
                    Emeralds[emeraldIndex].transform.GetChild(0).transform.rotation = new Quaternion(0, 0, 0, 1);
                    Emeralds[emeraldIndex].transform.position = Vector2.MoveTowards(Emeralds[emeraldIndex].transform.position, transform.position, 10f * FPStage.deltaTime);

                    // Check if this Emerald has reached the pivot point.
                    if (Emeralds[emeraldIndex].transform.position == transform.position)
                        EmeraldsDone[emeraldIndex] = true;
                }
            }

            else
            {
                // Loop through and move each Emerald outwards.
                for (int emeraldIndex = 0; emeraldIndex < Emeralds.Length; emeraldIndex++)
                {
                    Emeralds[emeraldIndex].transform.GetChild(0).transform.rotation = new Quaternion(0, 0, 0, 1);
                    Emeralds[emeraldIndex].transform.position += (Emeralds[emeraldIndex].transform.right * (10f * FPStage.deltaTime));

                    // Check if this Emerald is off screen.
                    if (!Emeralds[emeraldIndex].transform.GetChild(0).GetComponent<Renderer>().isVisible)
                        EmeraldsDone[emeraldIndex] = true;
                    else
                        EmeraldsDone[emeraldIndex] = false;
                }
            }

            // Check if every Emerald is flagged as done with its movement and destroy the object if so.
            if (EmeraldsDone.All(x => x))
                GameObject.Destroy(this.gameObject);
        }
    }
}
