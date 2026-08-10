using System.Linq;

namespace FP2_Sonic_Mod.CustomObjectScripts
{
    internal class OrbitingEmeralds : MonoBehaviour
    {
        // The array of Chaos Emeralds and whether they're done with their orbits.
        private GameObject[] Emeralds = new GameObject[7];
        private bool[] EmeraldsDone = new bool[7];

        void Start()
        {
            // Loop through and add each Emerald to the array, resetting the Done flags.
            for (int emeraldIndex = 0; emeraldIndex < Emeralds.Length; emeraldIndex++)
            {
                Emeralds[emeraldIndex] = transform.GetChild(emeraldIndex).gameObject;
                EmeraldsDone[emeraldIndex] = false;
            }
        }

        void Update()
        {
            // Rotate the pivot object.
            transform.Rotate(0, 0, -22.5f * FPStage.deltaTime);

            // Loop through and reset the rotation of each Emerald, moving it to the pivot point.
            for (int emeraldIndex = 0; emeraldIndex < Emeralds.Length; emeraldIndex++)
            {
                Emeralds[emeraldIndex].transform.rotation = new Quaternion(0, 0, 0, 1);
                Emeralds[emeraldIndex].transform.position = Vector2.MoveTowards(Emeralds[emeraldIndex].transform.position, transform.position, 10f * FPStage.deltaTime);

                if (Emeralds[emeraldIndex].transform.position == transform.position)
                    EmeraldsDone[emeraldIndex] = true;
            }

            // If every Emerald has reached the pivot point, then destroy this object.
            if (EmeraldsDone.All(x => x))
                GameObject.Destroy(this.gameObject);
        }
    }
}
