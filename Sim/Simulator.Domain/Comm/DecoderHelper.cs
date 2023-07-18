using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyDLab.Usv.Simulator.Domain.Comm
{
    public class DecoderHelper
    {
        #region Decoding for primitive

        public static float FloatDecoder(byte[] bytes, ref int offset)
        {
            float item = BitConverter.ToSingle(bytes, offset);
            offset += Common.SIZE_FLOAT;
            return item;
        }
        public static float[] FloatDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            float[] items = new float[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                items[i] = FloatDecoder(bytes, ref offset);
            }
            return items;
        }
        public static float[,] FloatDecoder(byte[] bytes, int arrayFirstMaxSize, int arraySecondMaxSize, ref int offset)
        {
            float[,] items = new float[arrayFirstMaxSize, arraySecondMaxSize];
            for (int i = 0; i < arrayFirstMaxSize; i++)
            {
                for (int j = 0; j < arraySecondMaxSize; j++)
                {
                    items[i, j] = FloatDecoder(bytes, ref offset);
                }
            }
            return items;
        }

        public static int IntDecoder(byte[] bytes, ref int offset)
        {
            int item = BitConverter.ToInt32(bytes, offset);
            offset += Common.SIZE_INT;
            return item;
        }
        public static int[] IntDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            int[] items = new int[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                items[i] = IntDecoder(bytes, ref offset);
            }
            return items;
        }

        public static uint UintDecoder(byte[] bytes, ref int offset)
        {
            uint item = BitConverter.ToUInt32(bytes, offset);
            offset += Common.SIZE_UINT;
            return item;
        }
        public static uint[] UintDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            uint[] items = new uint[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                items[i] = UintDecoder(bytes, ref offset);
            }
            return items;
        }

        public static char CharDecoder(byte[] bytes, ref int offset)
        {
            //char item = BitConverter.ToChar(bytes, offset);
            char item = Convert.ToChar(bytes[offset]);
            offset += Common.SIZE_CHAR;
            return item;
        }
        public static char[] CharDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            char[] items = new char[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                items[i] = CharDecoder(bytes, ref offset);
            }
            return items;
        }

        public static bool BoolDecoder(byte[] bytes, ref int offset)
        {
            bool item = Convert.ToBoolean(bytes[offset]);
            offset += Common.SIZE_BOOL1BYTE;
            return item;
        }

        public static bool BOOLDecoder(byte[] bytes, ref int offset)
        {
            int item = IntDecoder(bytes, ref offset);
            // offset += Common.SIZE_BOOL;
            return Convert.ToBoolean(item);
        }
        public static bool[] BOOLDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            int[] items = IntDecoder(bytes, maxSize, ref offset);
            bool[] _items = new bool[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                _items[i] = Convert.ToBoolean(items[i]);
            }

            return _items;
        }
        #endregion


        #region Encoding for class
        public static ST_POS ST_POSDecoder(byte[] bytes, ref int offset)
        {
            ST_POS item = new ST_POS();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_POS[] ST_POSDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            ST_POS[] items = new ST_POS[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                ST_POS item = new ST_POS();
                item.Deserialize(bytes, ref offset);
                items[i] = item;
            }
            return items;
        }

        public static ST_DGPS_POS ST_DGPS_POSDecoder(byte[] bytes, ref int offset)
        {
            ST_DGPS_POS item = new ST_DGPS_POS();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_DGPS_POS[] ST_DGPS_POSDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            ST_DGPS_POS[] items = new ST_DGPS_POS[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                ST_DGPS_POS item = new ST_DGPS_POS();
                item.Deserialize(bytes, ref offset);
                items[i] = item;
            }
            return items;
        }

        public static ST_VOYAGE_PLAN ST_VOYAGE_PLANDecoder(byte[] bytes, ref int offset)
        {
            ST_VOYAGE_PLAN item = new ST_VOYAGE_PLAN();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_VOYAGE_PLAN[] ST_VOYAGE_PLANDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            ST_VOYAGE_PLAN[] items = new ST_VOYAGE_PLAN[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                ST_VOYAGE_PLAN item = new ST_VOYAGE_PLAN();
                item.Deserialize(bytes, ref offset);
                items[i] = item;
            }
            return items;
        }
        public static ST_VESSEL_VELOCITY ST_VESSEL_VELOCITYDecoder(byte[] bytes, ref int offset)
        {
            ST_VESSEL_VELOCITY item = new ST_VESSEL_VELOCITY();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_VESSEL_MOTION ST_VESSEL_MOTIONDecoder(byte[] bytes, ref int offset)
        {
            ST_VESSEL_MOTION item = new ST_VESSEL_MOTION();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_VESSEL_INFO ST_VESSEL_INFODecoder(byte[] bytes, ref int offset)
        {
            ST_VESSEL_INFO item = new ST_VESSEL_INFO();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_VOYAGE_INFO ST_VOYAGE_INFODecoder(byte[] bytes, ref int offset)
        {
            ST_VOYAGE_INFO item = new ST_VOYAGE_INFO();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_ENVIRONMENT ST_ENVIRONMENTDecoder(byte[] bytes, ref int offset)
        {
            ST_ENVIRONMENT item = new ST_ENVIRONMENT();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_BASE_STATE ST_BASE_STATEDecoder(byte[] bytes, ref int offset)
        {
            ST_BASE_STATE item = new ST_BASE_STATE();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_OWNSHIP_INFO ST_OWNSHIP_STATEDecoder(byte[] bytes, ref int offset)
        {
            ST_OWNSHIP_INFO item = new ST_OWNSHIP_INFO();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_OBJECT_INFO ST_TARGET_SHIP_INFODecoder(byte[] bytes, ref int offset)
        {
            ST_OBJECT_INFO item = new ST_OBJECT_INFO();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_OBJECT_INFO[] ST_OBJECT_INFODecoder(byte[] bytes, int maxSize, ref int offset)
        {
            ST_OBJECT_INFO[] items = new ST_OBJECT_INFO[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                ST_OBJECT_INFO item = new ST_OBJECT_INFO();
                item.Deserialize(bytes, ref offset);
                items[i] = item;
            }
            return items;
        }

        public static ST_DATA_SA_OUT ST_SA_OUTDecoder(byte[] bytes, ref int offset)
        {
            ST_DATA_SA_OUT item = new ST_DATA_SA_OUT();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_CR_STATE ST_CR_STATEDecoder(byte[] bytes, ref int offset)
        {
            ST_CR_STATE item = new ST_CR_STATE();
            item.Deserialize(bytes, ref offset);
            return item;
        }
        public static ST_CR_STATE[] ST_CR_STATEDecoder(byte[] bytes, int maxSize, ref int offset)
        {
            ST_CR_STATE[] items = new ST_CR_STATE[maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                ST_CR_STATE item = new ST_CR_STATE();
                item.Deserialize(bytes, ref offset);
                items[i] = item;
            }
            return items;
        }
        #endregion

        public static ST_DATA_CR_OUT ST_DATA_CR_OUT_Decoder(byte[] bytes, ref int offset)
        {
            ST_DATA_CR_OUT item = new ST_DATA_CR_OUT();
            item.Deserialize(bytes, ref offset);
            return item;
        }

        public static ST_CA_COMMAND ST_CA_COMMAND_Decoder(byte[] bytes, ref int offset)
        {
            ST_CA_COMMAND item = new ST_CA_COMMAND();
            item.Deserialize(bytes, ref offset);
            return item;
        }

        public static ST_DATA_CA_OUT ST_DATA_CA_OUT_Decoder(byte[] bytes, ref int offset)
        {
            ST_DATA_CA_OUT item = new ST_DATA_CA_OUT();
            item.Deserialize(bytes, ref offset);
            return item;
        }

        public static ST_DATA_OP_TO_NM ST_DATA_OP_TO_NM_Decoder(byte[] bytes, ref int offset)
        {
            ST_DATA_OP_TO_NM item = new ST_DATA_OP_TO_NM();
            item.Deserialize(bytes, ref offset);
            return item;
        }

        public static ST_DATA_AUTO_PILOT ST_DATA_AUTO_PILOT_Decoder(byte[] bytes, ref int offset)
        {
            ST_DATA_AUTO_PILOT item = new ST_DATA_AUTO_PILOT();
            item.Deserialize(bytes, ref offset);
            return item;
        }
    }
}
