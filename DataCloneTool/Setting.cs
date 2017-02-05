using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace DataCloneTool
{
    public static class Setting
    {
        private static string _sourceConString;
        private static string _targetConString;

        public static string SourceConString
        {
            get 
            {
                if(!string.IsNullOrEmpty(_sourceConString)) return _sourceConString;

                _sourceConString = ConfigurationManager.ConnectionStrings["SourceDB"].ConnectionString;
                return _sourceConString;
            }
        }

        public static string TargeConString
        {
            get
            {
                if (!string.IsNullOrEmpty(_targetConString)) return _targetConString;

                _targetConString = ConfigurationManager.ConnectionStrings["TargetDB"].ConnectionString;
                return _targetConString;
            }
        }
    }
}
