﻿using System.Security.Cryptography;
using System.Text;

namespace JNPF.Common.Security
{
    /// <summary>
    /// 字符串Hash操作类.
    /// </summary>
    public static class HashHelper
    {
        /// <summary>
        /// 获取字符串的MD5哈希值，默认编码为<see cref="Encoding.UTF8"/>.
        /// </summary>
        public static string GetMd5(string value, Encoding? encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(value);
            return GetMd5(bytes);
        }

        /// <summary>
        /// 获取字节数组的MD5哈希值.
        /// </summary>
        public static string GetMd5(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            MD5 hash = MD5.Create();
            bytes = hash.ComputeHash(bytes);
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取字符串的SHA1哈希值，默认编码为<see cref="Encoding.UTF8"/>.
        /// </summary>
        public static string GetSha1(string value, Encoding? encoding = null)
        {
            StringBuilder sb = new StringBuilder();
            SHA1 hash = SHA1.Create();
            if (encoding == null)
                encoding = Encoding.UTF8;

            byte[] bytes = hash.ComputeHash(encoding.GetBytes(value));
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取字符串的Sha256哈希值，默认编码为<see cref="Encoding.UTF8"/>.
        /// </summary>
        public static string GetSha256(string value, Encoding? encoding = null)
        {
            StringBuilder sb = new StringBuilder();
            SHA256 hash = SHA256.Create();
            if (encoding == null)
                encoding = Encoding.UTF8;

            byte[] bytes = hash.ComputeHash(encoding.GetBytes(value));
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取字符串的Sha512哈希值，默认编码为<see cref="Encoding.UTF8"/>.
        /// </summary>
        public static string GetSha512(string value, Encoding? encoding = null)
        {
            StringBuilder sb = new StringBuilder();
            SHA512 hash = SHA512.Create();
            if (encoding == null)
                encoding = Encoding.UTF8;

            byte[] bytes = hash.ComputeHash(encoding.GetBytes(value));
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }
    }
}