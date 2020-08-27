using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AAR
{
    public class CSServoController : MonoBehaviour
    {
        public int ID;
        public ServoInterface.Payload TestCmd;

        private void Start()
        {

        }

        private void Update()
        {

        }

        public void ProcessCommand(ServoInterface.Payload _cmd)
        {
            ServoInterface.ProcessCommand(_cmd);
        }

    }
}

