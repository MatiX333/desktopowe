using System;

namespace R11_Zadanie_11_2
{
    class Zadanie
    {
        public string Nazwa { get; set; }
        public string Opis { get; set; }
        public int Priorytet { get; set; }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", Nazwa, Opis, Priorytet);
        }
    }
}
