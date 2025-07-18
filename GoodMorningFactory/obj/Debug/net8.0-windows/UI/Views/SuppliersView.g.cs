﻿#pragma checksum "..\..\..\..\..\UI\Views\SuppliersView.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5EDC85E953EDE6C37F4D78886823F25EAE9BFA59"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.Converters;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace GoodMorningFactory.UI.Views {
    
    
    /// <summary>
    /// SuppliersView
    /// </summary>
    public partial class SuppliersView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 31 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox SearchTextBox;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox StatusFilterComboBox;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid SuppliersDataGrid;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button PreviousPageButton;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock PageInfoTextBlock;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button NextPageButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.7.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/GoodMorningFactory;component/ui/views/suppliersview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.7.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 27 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.AddSupplierButton_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 28 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ExportToCsvButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.SearchTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 31 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            this.SearchTextBox.KeyUp += new System.Windows.Input.KeyEventHandler(this.SearchTextBox_KeyUp);
            
            #line default
            #line hidden
            return;
            case 4:
            this.StatusFilterComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 33 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            this.StatusFilterComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.Filter_Changed);
            
            #line default
            #line hidden
            return;
            case 5:
            this.SuppliersDataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 9:
            this.PreviousPageButton = ((System.Windows.Controls.Button)(target));
            
            #line 86 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            this.PreviousPageButton.Click += new System.Windows.RoutedEventHandler(this.PreviousPageButton_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.PageInfoTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 11:
            this.NextPageButton = ((System.Windows.Controls.Button)(target));
            
            #line 90 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            this.NextPageButton.Click += new System.Windows.RoutedEventHandler(this.NextPageButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.7.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 6:
            
            #line 68 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.EditButton_Click);
            
            #line default
            #line hidden
            break;
            case 7:
            
            #line 69 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ViewDetailsButton_Click);
            
            #line default
            #line hidden
            break;
            case 8:
            
            #line 70 "..\..\..\..\..\UI\Views\SuppliersView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.DeleteButton_Click);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

