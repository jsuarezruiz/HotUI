﻿using System;
using System.Collections.Generic;

namespace HotUI.Samples
{
    public class BasicNavigationTestView : View
    {
        [Body]
        View body() => new NavigationView
        {
            new VStack()
            {
                new Button("Navigate!",()=>{
                    Navigation.Navigate(new BasicTestView());
                })
            }
        };
    }



}