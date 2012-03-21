/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Kelp.XInclude
{
	internal struct FallbackState
	{
		//Fallback is being processed
		public bool Fallbacking;
		//xi:fallback element depth
		public int FallbackDepth;
		//Fallback processed flag
		public bool FallbackProcessed;
	}

	/// <summary>
	/// XIncludingReader state machine.    
	/// </summary>
	/// <author>Oleg Tkachenko, http://www.xmllab.net</author>
	internal enum XIncludingReaderState
	{
		//Default state
		Default,
		//xml:base attribute is being exposed
		ExposingXmlBaseAttr,
		//xml:base attribute value is being exposed
		ExposingXmlBaseAttrValue,
		//xml:lang attribute is being exposed
		ExposingXmlLangAttr,
		//xml:lang attribute value is being exposed
		ExposingXmlLangAttrValue
	}
}
