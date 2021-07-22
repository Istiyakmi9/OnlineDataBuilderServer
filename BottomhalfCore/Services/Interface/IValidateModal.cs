using BottomhalfCore.BottomhalfModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BottomhalfCore.Services.Interface
{
    public interface IValidateModal<T>
    {
        ServiceResult ValidateModalFieldsService(Type ObjectName, dynamic ReferencedObject);
    }
}
