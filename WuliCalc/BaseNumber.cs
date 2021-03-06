﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WuliCalc
{
    public class BaseNumber : IComparable<BaseNumber>
    {
        protected UInt64 _N;
        protected int _Width = 64;
        protected bool _Signed = false;

        public int Width => _Width;
        public bool Signed => _Signed;

        public const int DefaultWidth = 32;

        public BaseNumber()
        {
            _N = 0;
        }

        public BaseNumber(UInt64 n)
        {
            _Width = 64;
            SetBaseData(n);
        }

        public BaseNumber(int n)
        {
            _Width = 32;
            SetBaseData((UInt64)n);
        }

        public BaseNumber(UInt64 n, int width)
        {
            _Width = width;
            SetBaseData(n);
        }

        public BaseNumber(UInt64 n, int width, bool signed)
        {
            _Width = width;
            _Signed = signed;
            SetBaseData(n);
        }

        public BaseNumber(string s, int basement, int width, bool signed)
        {
            _Width = width;
            _Signed = signed;
            if(String.IsNullOrWhiteSpace(s))
            {
                s = "0";
            }
            switch(basement)
            {
                case 10:
                    SetBaseData(UInt64.Parse(s));
                    break;
                case 16:
                    SetBaseData(Hex2Dec(s));
                    break;
            }
        }

        #region Base Function
        protected void SetBaseData(UInt64 n)
        {
            _N = n;
        }

        protected void SetBaseData(UInt32 n)
        {
            _N = (UInt64)n;
        }

        public UInt64 GetData()
        {
            return GetBaseData();
        }

        public UInt64 GetData(int width)
        {
            string s = GetBaseData().ToString();
            char[] bs = s.ToCharArray();
            throw new NotImplementedException();
        }

        protected UInt64 ExpandTo(int width)
        {
            UInt64 res = 0;
            if(_Signed == false)
            {
                res = GetBaseData();
            }
            else
            {
                throw new NotImplementedException();
            }
            _Width = width;
            return res;
        }

        protected UInt64 TruncateTo(int width)
        {
            _Width = width;
            return GetBaseData() & GetMask(width);
        }

        protected UInt64 SaturateTo(int width)
        {
            UInt64 res = 0;
            if(_Signed == false)
            {
                if((GetBaseData() & ~GetMask(width)) > 0)
                {
                    res = GetMask(width);
                }
                else
                {
                    res = TruncateTo(width);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            _Width = width;
            return res;
        }

        public void SetNewWidth(int width)
        {
            if(width > Width)
            {
                SetBaseData(ExpandTo(width));
            }
            else
            {
                SetBaseData(SaturateTo(width));
            }
        }

        public void Truncate(int width)
        {
            SetBaseData(TruncateTo(width));
        }

        public void Saturate(int width)
        {
            SetBaseData(SaturateTo(width));
        }

        protected UInt64 GetBaseData()
        {
            return _N;
        }

        /// <summary>
        /// Get mask(1) from 0 to WIDTH
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public static UInt64 GetMask(int width)
        {
            UInt64 res = 0;

            if (width > 64) width = 64;
            else if (width < 1) width = 1;

            if(width == 64)
            {
                res = UInt64.MaxValue;
            }
            else
            {
                res = (1ul << width) - 1;
            }
            return res;
        }

        /// <summary>
        /// Get mask(1) from LSB to LSB + WIDTH
        /// </summary>
        /// <param name="lsb"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static UInt64 GetMask(int lsb, int width)
        {
            if (lsb >= 64) lsb = 63;
            if (lsb + width > 64) width = 64 - lsb;

            UInt64 m = GetMask(lsb + width);
            if (lsb == 0) return m;
            else
            {
                UInt64 inv = ~GetMask(lsb - 1);
                return m & inv;
            }
        }

        #endregion

        #region OPERATOR
        public static BaseNumber operator +(BaseNumber lhs, BaseNumber rhs)
        {
            return new BaseNumber(lhs.GetBaseData() + rhs.GetBaseData());
        }

        public static BaseNumber operator -(BaseNumber lhs, BaseNumber rhs)
        {
            return new BaseNumber(lhs.GetBaseData() - rhs.GetBaseData());
        }

        public static BaseNumber operator >>(BaseNumber lhs, int shift)
        {
            return new BaseNumber(lhs.GetBaseData() >> shift);
        }

        public static BaseNumber operator <<(BaseNumber lhs, int shift)
        {
            return new BaseNumber(lhs.GetBaseData() << shift);
        }

        public static bool operator ==(BaseNumber lhs, BaseNumber rhs)
        {
            return lhs.GetBaseData() == rhs.GetBaseData();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator !=(BaseNumber lhs, BaseNumber rhs)
        {
            return lhs.GetBaseData() != rhs.GetBaseData();
        }

        public static int operator >(BaseNumber lhs, BaseNumber rhs)
        {
            return lhs.CompareTo(rhs);
        }

        public static int operator <(BaseNumber lhs, BaseNumber rhs)
        {
            return rhs.CompareTo(lhs);
        }

        public static BaseNumber operator |(BaseNumber lhs, BaseNumber rhs)
        {
            lhs.OrWith(rhs);
            return lhs;
        }

        public static BaseNumber operator ^(BaseNumber lhs, BaseNumber rhs)
        {
            lhs.XorWith(rhs);
            return lhs;
        }

        public static BaseNumber operator &(BaseNumber lhs, BaseNumber rhs)
        {
            lhs.AndWith(rhs);
            return lhs;
        }

        public static BaseNumber operator ~(BaseNumber rhs)
        {
            rhs.XorWith(new BaseNumber(GetMask(rhs.Width)));
            return rhs;
        }
        #endregion

        public int CompareTo(BaseNumber cmp)
        {
            int res = 0;
            if(this.GetBaseData() > cmp.GetBaseData())
            {
                res = 1;
            }
            else if(this.GetBaseData() > cmp.GetBaseData())
            {
                res = 0;
            }
            else
            {
                res = -1;
            }
            return res;
        }

        public void OrWith(BaseNumber n)
        {
            if(n.Width < this.Width)
            {
                n.ExpandTo(this.Width);
            }
            SetBaseData(n.GetData() | this.GetData());
        }

        public void XorWith(BaseNumber n)
        {
            if(n.Width < this.Width)
            {
                n.ExpandTo(this.Width);
            }
            SetBaseData(n.GetData() ^ this.GetData());
        }

        public void AndWith(BaseNumber n)
        {
            if(n.Width < this.Width)
            {
                n.ExpandTo(this.Width);
            }
            SetBaseData(n.GetData() & this.GetData());
        }

        public void RevertBit(int posi)
        {
            if (posi >= this.Width)
            {
                return;
            }
            UInt64 mask = 1ul << posi;
            XorWith(new BaseNumber(mask));
        }

        #region String Fomart
        public static UInt64 Hex2Dec(string hex)
        {
            UInt64 dec = 0;
            for (int i = 0, j = hex.Length - 1; i < hex.Length; i++)
            {
                dec += (UInt64)HexChar2Value(hex.Substring(i, 1)) * ((UInt64)Math.Pow(16, j));
                j--;
            }
            return dec;
        }

        public static int HexChar2Value(string hexChar)
        {
            switch (hexChar)
            {
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    return Convert.ToInt32(hexChar);
                case "a":
                case "A":
                    return 10;
                case "b":
                case "B":
                    return 11;
                case "c":
                case "C":
                    return 12;
                case "d":
                case "D":
                    return 13;
                case "e":
                case "E":
                    return 14;
                case "f":
                case "F":
                    return 15;
                default:
                    return 0;
            }
        }

        protected char[] GetFixedWidthDecChars(int msb, int lsb)
        {
            if(lsb < 0)
            {
                throw new ArgumentOutOfRangeException("LSB value is less than 0");
            }
            if(msb >= _Width)
            {
                throw new ArgumentOutOfRangeException("MSB value is larger than data width");
            }
            UInt64 mask = GetMask(lsb, msb - lsb + 1);
            UInt64 masked = this.GetBaseData() & mask;
            if(lsb > 0)
            {
                masked = masked >> lsb;
            }
            return masked.ToString().ToCharArray();
        }

        protected char[] GetFixedWidthHexChars(int msb, int lsb)
        {
            string cs = new String(GetFixedWidthDecChars(msb, lsb));
            UInt64 n = UInt64.Parse(cs);
            UInt64 div = n;
            UInt64 rem = 0;
            List<int> hex = new List<int>();
            while(div > 0)
            {
                rem = div % 16;
                hex.Add((int)rem);
                div = div / 16;
            }
            string res = "";
            foreach(int h in hex)
            {
                string s = h.ToString("X");
                res = s + res;
            }
            return res.ToCharArray();
        }

        protected string ExpandStr(string input, int width)
        {
            StringBuilder res = new StringBuilder(input);
            if(input.Length >= width)
            {
                return res.ToString();
            }
            res.Insert(0, "0", width - input.Length);
            return res.ToString();
        }

        protected UInt64 HexChar2Dec(char hex)
        {
            byte n = (byte)hex;
            if(n > 47 && n < 58) // 0-9
            {
                n -= 48;
            }
            else if(n > 64 && n < 71) // A-F
            {
                n -= 87;
            }
             else if(n > 96 && n < 103) // a-f
            {
                n -= 87;
            }
            else
            {
                n = 0;
            }
            return (UInt64)n;
        }
        
        protected string HexChar2Bin(char hex)
        {
            string res = "";
            switch(hex)
            {
                case '0':
                    res = "0000";
                    break;
                case '1':
                    res = "0001";
                    break;
                case '2':
                    res = "0010";
                    break;
                case '3':
                    res = "0011";
                    break;
                case '4':
                    res = "0100";
                    break;
                case '5':
                    res = "0101";
                    break;
                case '6':
                    res = "0110";
                    break;
                case '7':
                    res = "0111";
                    break;
                case '8':
                    res = "1000";
                    break;
                case '9':
                    res = "1001";
                    break;
                case 'a':
                    res = "1010";
                    break;
                case 'A':
                    res = "1010";
                    break;
                case 'b':
                    res = "1011";
                    break;
                case 'B':
                    res = "1011";
                    break;
                case 'c':
                    res = "1100";
                    break;
                case 'C':
                    res = "1100";
                    break;
                case 'd':
                    res = "1101";
                    break;
                case 'D':
                    res = "1101";
                    break;
                case 'e':
                    res = "1110";
                    break;
                case 'E':
                    res = "1110";
                    break;
                case 'f':
                    res = "1111";
                    break;
                case 'F':
                    res = "1111";
                    break;
                default:
                    res = "0000";
                    break;
            }
            return res;
        }

        protected char[] GetFixedWidthBinChars(int msb, int lsb)
        {
            char[] cs = GetFixedWidthHexChars(msb, lsb);
            string res = "";
            foreach(char c in cs)
            {
                res += HexChar2Bin(c);
            }
            return res.ToCharArray();
        }

        public override string ToString()
        {
            return ToString("D");
        }

        public string ToString(string format)
        {
            string res = "";
            if(format.Equals("D"))
            {
                res = new String(GetFixedWidthDecChars(Width - 1, 0));
            }
            else if(format == "X")
            {
                res = new String(GetFixedWidthHexChars(Width - 1, 0));
                int realWidth = (Width % 4 == 0) ? (Width / 4) : (Width / 4 + 1);
                res = ExpandStr(res, realWidth);
            }
            else if(format == "B")
            {
                res = new String(GetFixedWidthBinChars(Width - 1, 0));
                res = ExpandStr(res, Width);
            }
            return res;
        }

        public char GetBit(int index)
        {
            string s = ToString("B");
            if(index >= Width || index >= s.Length)
            {
                throw new ArgumentOutOfRangeException(index.ToString());
            }
            return s.ToCharArray()[index];
        }

        #endregion
    }
}
