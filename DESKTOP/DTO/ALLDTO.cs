using System;
using System.Collections.Generic;
using System.Text;

namespace DESKTOP.DTO
{
    class ALLDTO
    {
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string CategoryName { get; set; }

        }

        public class EditProductRequest()
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string CategoryName { get; set; }

        };
    }
}
