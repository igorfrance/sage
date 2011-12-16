namespace Kelp.SyntaxHighlighting
{
	using System;

	/// <summary>
	/// Defines the JavaScript syntax highlighting elements.
	/// </summary>
	public class JavaScriptDefinition : LanguageDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JavaScriptDefinition"/> class.
		/// </summary>
		public JavaScriptDefinition() : base("js")
		{
			this.Comments.Add(@"(//[^\n]*)\n");
			this.Comments.Add(@"(/\*[\s\S]*?\*/)");
			this.Quotes.Add(@"(""|')[^\1\\]*(?:\\.[^\1\\]*)*\1");

			this.Expressions.Add(new ExpressionGroup("keyword",
				"abstract|boolean|break|byte|case|catch|char|class|const|continue|default|delete|do|double|else|" +
				"extends|false|final|finally|float|for|function|goto|if|implements|import|in|instanceof|int|" +
				"interface|long|native|new|null|package|private|protected|public|return|short|static|super|switch|" +
				"synchronized|this|throw|throws|transient|true|try|typeof|var|void|while|with"
			));

			this.Expressions.Add(new ExpressionGroup("builtin1",
				"ActiveXObject|window|document|implementation|arguments|Array|Boolean|Date|Enumerator|Error|" + 
				"forms|Frame|frames|Function|Number|Math|Object|RegExp|String|Type|VBArray|XMLHttpRequest"
			));

			this.Expressions.Add(new ExpressionGroup("builtin2",
				"abort|abs|acos|addEventListener|appendChild|appendData|apply|asin|atan|atan2|atEnd|attachEvent|" +
				"blur|call|ceil|charAt|charCodeAt|click|cloneNode|clearInterval|clearTimeout|compile|concat|" +
				"cos|createAttribute|createAttributeNS|createCDATASection|createComment|createDocument|" +
				"createDocumentFragment|createDocumentType|createElement|createElementNS|createEntityReference|" +
				"createProcessingInstruction|createTextNode|decodeURI|decodeURIComponent|deleteData|detachEvent|" +
				"dimensions|dispatchEvent|encodeURI|encodeURIComponent|escape|eval|exec|exp|fixed|floor|focus|" +
				"fromCharCode|getAllResponseHeaders|getAttribute|getAttributeNode|getAttributeNodeNS|getAttributeNS|" +
				"getComputedStyle|getDate|getDay|getElementById|getElementsByName|getElementsByTagName|getElementsByTagNameNS|" +
				"getFullYear|getHours|getItem|getMilliseconds|getMinutes|getMonth|getNamedItem|getNamedItemNS|" +
				"getResponseHeader|getSeconds|getTime|getTimezoneOffset|getUTCDate|getUTCDay|getUTCFullYear|" +
				"getUTCHours|getUTCMilliseconds|getUTCMinutes|getUTCMonth|getUTCSeconds|getVarDate|getYear|hasAttribute|" +
				"hasAttributeNS|hasAttributes|hasChildNodes|hasFeature|hasOwnProperty|importNode|indexOf|insertBefore|" +
				"insertData|isFinite|isNaN|isPrototypeOf|isSupported|item|join|lastIndexOf|lbound|localeCompare|log|match|" +
				"max|min|moveFirst|moveNext|normalize|open|parse|parseFloat|parseInt|pop|pow|push|random|removeAttribute|" +
				"removeAttributeNode|removeAttributeNS|removeChild|removeEventListener|removeNamedItem|removeNamedItemNS|" +
				"replace|replaceChild|replaceData|reverse|round|scrollIntoView|search|send|sendAsBinary|setInterval|" +
				"setTimeout|setAttribute|setAttributeNode|setAttributeNodeNS|setAttributeNS|setDate|setFullYear|setHours|" +
				"setMilliseconds|setMinutes|setMonth|setNamedItem|setNamedItemNS|setRequestHeader|setSeconds|setTime|" +
				"setUTCDate|setUTCFullYear|setUTCHours|setUTCMilliseconds|setUTCMinutes|setUTCMonth|setUTCSeconds|setYear|" +
				"shift|sin|slice|sort|splice|split|splitText|sqrt|substr|substring|substringData|supports|tan|test|toArray|" +
				"toDateString|toExponential|toFixed|toGMTString|toLocaleDateString|tolocaleLowerCase|toLocaleString|" +
				"toLocaleTimeString|tolocaleUpperCase|toLowerCase|toPrecision|toString|toUpperCase|toUTCString|ubound|" +
				"unescape|unshift|UTC|valueOf"
			));

			this.Expressions.Add(new ExpressionGroup("builtin3",
				"$1|$2|$3|$4|$5|$6|$7|$8|$9|arguments|attributes|callee|caller|childNodes|className|clientWidth|clientHeight|" + 
				"dir|constructor|data|description|documentElement|domain|E|firstChild|global|id|ignoreCase|implementation|" + 
				"index|Infinity|input|innerHTML|lang|lastChild|lastIndex|lastMatch|lastParen|leftContext|length|LN10|LN2|" + 
				"localName|location|LOG10E|LOG2E|MAX_VALUE|message|MIN_VALUE|multiline|name|namespaceURI|namespaceURI|NaN|" + 
				"NEGATIVE_INFINITY|nextSibling|nodeName|nodeType|nodeValue|number|offsetLeft|offsetTop|offsetParent|offsetWidth|" + 
				"offsetHeight|ownerDocument|parentNode|PI|POSITIVE_INFINITY|prefix|previousSibling|AbsolutePath|propertyIsEnumerable|" + 
				"prototype|readyState|rightContext|scrollLeft|scrollTop|scrollHeight|scrollWidth|style|source|SQRT1_2|SQRT2|" + 
				"tabIndex|tagName|target|title|undefined|value"
			));
		}
	}
}
