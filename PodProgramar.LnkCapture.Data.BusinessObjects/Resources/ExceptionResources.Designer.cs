﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PodProgramar.LnkCapture.Data.BusinessObjects.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ExceptionResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExceptionResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PodProgramar.LnkCapture.Data.BusinessObjects.Resources.ExceptionResources", typeof(ExceptionResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 😢 Não consigo te enviar uma mensagem em particular. Acho que você me bloqueou..
        /// </summary>
        internal static string ApiRequestException {
            get {
                return ResourceManager.GetString("ApiRequestException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Para recuperar os links é preciso iniciar uma conversa comigo em uma mensagem privada. Uma vez iniciada volte aqui e execute o comando /linksUrl@LnkCaptureBot que te enviarei no chat privado um link para acesso..
        /// </summary>
        internal static string ChatNotInitiatedException {
            get {
                return ResourceManager.GetString("ChatNotInitiatedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Identificação inválida.
        /// </summary>
        internal static string InvalidId {
            get {
                return ResourceManager.GetString("InvalidId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 😓 Alguma coisa deu errado aqui! Não consigo executar o que você me pediu. Tenta de novo mais tarde por favor..
        /// </summary>
        internal static string UnknownException {
            get {
                return ResourceManager.GetString("UnknownException", resourceCulture);
            }
        }
    }
}