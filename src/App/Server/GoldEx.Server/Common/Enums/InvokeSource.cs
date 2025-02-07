﻿using System.ComponentModel.DataAnnotations;

namespace GoldEx.Server.Common.Enums;

public enum InvokeSource
{
    [Display(Name = "ورود")]
    Login,

    [Display(Name = "ثبت نام")]
    Register
}