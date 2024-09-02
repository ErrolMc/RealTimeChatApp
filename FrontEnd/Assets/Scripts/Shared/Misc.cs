using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ChatApp.Shared.TableDataSimple;

namespace ChatApp.Shared.Misc
{
    [Serializable]
    public class GenericResponseData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}