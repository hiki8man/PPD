﻿//--------------------------------------------------------
// This code is generated by AutoFastGenerator.
// You should not modify the code.
//--------------------------------------------------------

namespace FlowScriptEngineBasicExtension.FlowSourceObjects.Enumerable
{
    public partial class RangeFlowSourceObject
    {
        public override object GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "Range":
                    return Range;
                default:
                    return null;
            }
        }

        protected override void SetPropertyValue(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "EndIndex":
                    EndIndex = (System.Int32)value;
                    break;
            }
        }

    }
}