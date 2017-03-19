using UnityEngine;

namespace Experimental
{
    public class DeviceProvider : IInputProvider
    {
        public void Get(InputParameters parameters)
        {
            parameters.horz = Input.GetAxisRaw("Horizontal");
            parameters.vert = Input.GetAxisRaw("Vertical");
        }
    }
}
