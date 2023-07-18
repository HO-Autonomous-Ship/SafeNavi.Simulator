using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyDLab.Usv.Simulator.Domain.Comm
{
    public class EncoderHelper
    {
        #region Encoding for primitive
        public static void FloatEncoder(float item, ref byte[] bytes)
        {
            Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_FLOAT);
            Buffer.BlockCopy(BitConverter.GetBytes(item), 0, bytes, bytes.Length - Common.SIZE_FLOAT, Common.SIZE_FLOAT);
        }
        public static void FloatEncoder(float[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    FloatEncoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_FLOAT * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_FLOAT * maxSize);
            }
        }
        public static void FloatEncoder(float[,] items, int arrayFirstMaxSize, int arraySecondMaxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (float item in items)
                {
                    FloatEncoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_FLOAT * (arrayFirstMaxSize - items.GetLength(0)) * (arraySecondMaxSize - items.GetLength(1)));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_FLOAT * arrayFirstMaxSize * arraySecondMaxSize);
            }
        }

        public static void IntEncoder(int item, ref byte[] bytes)
        {
            Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_INT);
            Buffer.BlockCopy(BitConverter.GetBytes(item), 0, bytes, bytes.Length - Common.SIZE_INT, Common.SIZE_INT);
        }
        public static void IntEncoder(int[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    IntEncoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_INT * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_INT * maxSize);
            }
        }

        public static void UintEncoder(uint item, ref byte[] bytes)
        {
            Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_UINT);
            Buffer.BlockCopy(BitConverter.GetBytes(item), 0, bytes, bytes.Length - Common.SIZE_UINT, Common.SIZE_UINT);
        }
        public static void UintEncoder(uint[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (uint item in items)
                {
                    UintEncoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_UINT * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_UINT * maxSize);
            }
        }

        public static void CharEncoder(char item, ref byte[] bytes)
        {
            Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_CHAR);
            byte[] temp = new byte[] { Convert.ToByte(item) };
            Buffer.BlockCopy(temp, 0, bytes, bytes.Length - Common.SIZE_CHAR, Common.SIZE_CHAR);
        }
        public static void CharEncoder(char[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (char item in items)
                {
                    CharEncoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_CHAR * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_CHAR * maxSize);
            }
        }

        public static void BoolEncoder(bool item, ref byte[] bytes)
        {
            Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_BOOL1BYTE);
            byte[] temp = new byte[] { Convert.ToByte(item) };
            Buffer.BlockCopy(temp, 0, bytes, bytes.Length - Common.SIZE_BOOL1BYTE, Common.SIZE_BOOL1BYTE);
        }

        public static void BOOLEncoder(bool item, ref byte[] bytes)
        {
            IntEncoder(Convert.ToInt32(item), ref bytes);
        }
        public static void BOOLEncoder(bool[] items, int maxSize, ref byte[] bytes)
        {
            int[] _items = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                _items[i] = Convert.ToInt32(items[i]);
            }
            IntEncoder(_items, maxSize, ref bytes);
        }
        #endregion













        #region Encoding for class

        public static void ST_POS_Encoder(ST_POS item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_POS);
        }
        public static void ST_POS_Encoder(ST_POS[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (ST_POS item in items)
                {
                    ST_POS_Encoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_POS * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_POS * maxSize);
            }
        }
        public static void ST_DGPS_POS_Encoder(ST_DGPS_POS item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_DGPS_POS);
        }
        public static void ST_DGPS_POS_Encoder(ST_DGPS_POS[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (ST_DGPS_POS item in items)
                {
                    ST_DGPS_POS_Encoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_DGPS_POS * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_DGPS_POS * maxSize);
            }
        }

        public static void ST_VOYAGE_PLAN_Encoder(ST_VOYAGE_PLAN item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_VOYAGE_PLAN);
        }

        public static void ST_VESSEL_VELOCITY_Encoder(ST_VESSEL_VELOCITY item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_VESSEL_VELOCITY);
        }


        public static void ST_ENVIRONMENT_Encoder(ST_ENVIRONMENT item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_ENVIRONMENT);
        }

        public static void ST_BASE_STATE_Encoder(ST_BASE_STATE item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_BASE_STATE);
        }

        public static void ST_OWNSHIP_INFO_Encoder(ST_OWNSHIP_INFO item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_OWNSHIP_INFO);
        }
        public static void ST_OWNSHIP_INFO_Encoder(ST_OWNSHIP_INFO[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (ST_OWNSHIP_INFO item in items)
                {
                    ST_OWNSHIP_INFO_Encoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_OWNSHIP_INFO * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_OWNSHIP_INFO * maxSize);
            }
        }

        public static void ST_OBJECT_INFO_Encoder(ST_OBJECT_INFO item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_OBJECT_INFO);
        }
        public static void ST_OBJECT_INFO_Encoder(ST_OBJECT_INFO[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (ST_OBJECT_INFO item in items)
                {
                    ST_OBJECT_INFO_Encoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_OBJECT_INFO * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_OBJECT_INFO * maxSize);
            }
        }

        public static void ST_DATA_SA_OUT_Encoder(ST_DATA_SA_OUT item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_DATA_SA_OUT);
        }

        public static void ST_CR_STATE_Encoder(ST_CR_STATE item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_CR_STATE);
        }
        public static void ST_CR_STATE_Encoder(ST_CR_STATE[] items, int maxSize, ref byte[] bytes)
        {
            if (items != null)
            {
                foreach (ST_CR_STATE item in items)
                {
                    ST_CR_STATE_Encoder(item, ref bytes);
                }
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_CR_STATE * (maxSize - items.Length));
            }
            else
            {
                Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_CR_STATE * maxSize);
            }
        }

        public static void ST_VESSEL_INFO_Encoder(ST_VESSEL_INFO item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_VESSEL_INFO);
        }

        public static void ST_VOYAGE_INFO_Encoder(ST_VOYAGE_INFO item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_VOYAGE_INFO);
        }

        public static void ST_VESSEL_MOTION_Encoder(ST_VESSEL_MOTION item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_VESSEL_MOTION);
        }

        #endregion



        public static void ST_DATA_CR_OUT_Encoder(ST_DATA_CR_OUT item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_DATA_CR_OUT);
        }

        public static void ST_CA_COMMAND_Encoder(ST_CA_COMMAND item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_CA_COMMAND);
        }

        public static void ST_DATA_CA_OUT_Encoder(ST_DATA_CA_OUT item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_ST_DATA_CA_OUT);
        }

        public static void ST_DATA_OP_TO_NM_Encoder(ST_DATA_OP_TO_NM item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_DATA_OP_TO_NM);
        }

        public static void ST_DATA_AUTO_PILOT_Encoder(ST_DATA_AUTO_PILOT item, ref byte[] bytes)
        {
            if (item != null) item.Serialize(ref bytes);
            else Array.Resize<byte>(ref bytes, bytes.Length + Common.SIZE_DATA_AUTO_PILOT);
        }
    }
}
