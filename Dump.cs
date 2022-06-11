namespace DevTools
{
    unsafe public static class Dump
    {
        public static string Bits(byte value, bool spaces = true)
        {
            char* result = stackalloc char[8];

            for (int i = 0; i < 8; i++)
            {
                result[i] = (char)(((value >> (7 - i)) & 1) + '0');
            }

            return new string(result, 0, 8).Insert(4, spaces ? " " : "");
        }

        public static string Bits<T>(T value, bool spaces = true)
            where T : unmanaged
        {
            return Bits(&value, sizeof(T), spaces);
        }

        public static string Bits(void* ptr, int bytes, bool spaces = true)
        {
Assert.IsNotNull(ptr);
Assert.IsNonNegative(bytes);

            byte* address = (byte*)ptr;
            string result = string.Empty;

            while (bytes != 0)
            {
                result = result.Insert(0, Bits(*address, spaces) + (spaces ? " " : ""));

                address++;
                bytes--;
            }

            return result;
        }


        private static string Hex(byte value, char* HEX_VALUES)
        {
            char* result = stackalloc char[] { HEX_VALUES[value >> 4], HEX_VALUES[value & 0b1111] };

            return new string(result, 0, 2);
        }

        public static string Hex(byte value)
        {
            char* HEX_VALUES = stackalloc char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            return Hex(value, HEX_VALUES);
        }

        public static string Hex<T>(T value, bool spaces = true)
            where T : unmanaged
        {
            return Hex(&value, sizeof(T), spaces);
        }

        public static string Hex(void* ptr, int bytes, bool spaces = true)
        {
Assert.IsNotNull(ptr);
Assert.IsNonNegative(bytes);
            
            char* HEX_VALUES = stackalloc char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            byte* address = (byte*)ptr;
            uint iterations = 0;
            string result = string.Empty;

            while (iterations != bytes)
            {
                string block = Hex(*address, HEX_VALUES);
                if (spaces && (iterations != 0) & (iterations % 2 == 0))
                {
                    block += " ";
                }
                result = result.Insert(0, block);

                address++;
                iterations++;
            }

            return result;
        }
    }
}