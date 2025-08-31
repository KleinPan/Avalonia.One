using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace One.Toolbox.ViewModels.StringNodify;

public interface IOperation
{
    double Execute(params double[] operands);
}
