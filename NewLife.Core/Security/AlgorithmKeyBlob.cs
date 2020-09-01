﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewLife.Security
{
    enum AlgorithmKeyBlob
    {
        ECDH_PUBLIC_P256 = 0x314B4345,
        ECDH_PRIVATE_P256 = 0x324B4345,
        ECDH_PUBLIC_P384 = 0x334B4345,
        ECDH_PRIVATE_P384 = 0x344B4345,
        ECDH_PUBLIC_P521 = 0x354B4345,
        ECDH_PRIVATE_P521 = 0x364B4345,
        ECDH_PUBLIC_GENERIC = 0x504B4345,
        ECDH_PRIVATE_GENERIC = 0x564B4345,
        ECDSA_PUBLIC_P256 = 0x31534345,
        ECDSA_PRIVATE_P256 = 0x32534345,
        ECDSA_PUBLIC_P384 = 0x33534345,
        ECDSA_PRIVATE_P384 = 0x34534345,
        ECDSA_PUBLIC_P521 = 0x35534345,
        ECDSA_PRIVATE_P521 = 0x36534345,
        ECDSA_PUBLIC_GENERIC = 0x50444345,
        ECDSA_PRIVATE_GENERIC = 0x56444345,
        RSAPUBLIC = 0x31415352,
        RSAPRIVATE = 0x32415352,
        RSAFULLPRIVATE = 0x33415352,
        KEY_DATA_BLOB = 0x4D42444B
    }
}