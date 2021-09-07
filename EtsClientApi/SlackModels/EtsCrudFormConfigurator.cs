using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{


    


    public class EtsCrudFormConfigurator
    {

        public enum EtsCrudFormType { create, update, delete };


        public EtsCrudFormType ControlType { get; set; }

        public EtsCrudFormConfigurator(EtsCrudFormType controlType)
        {
            this.ControlType = controlType;


        }




    }
}
