using System.Collections.Generic;

namespace e2kp
{
    class Category
    {
        public string Name { get; set; }

        public List<Card> Cards { get; protected set; } = new List<Card>();
    }
}
