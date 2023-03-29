"use strict";(self.webpackChunkthingsgateway=self.webpackChunkthingsgateway||[]).push([[282],{3905:(e,t,n)=>{n.d(t,{Zo:()=>l,kt:()=>u});var r=n(7294);function a(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function o(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function i(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?o(Object(n),!0).forEach((function(t){a(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function c(e,t){if(null==e)return{};var n,r,a=function(e,t){if(null==e)return{};var n,r,a={},o=Object.keys(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||(a[n]=e[n]);return a}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(r=0;r<o.length;r++)n=o[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(a[n]=e[n])}return a}var d=r.createContext({}),s=function(e){var t=r.useContext(d),n=t;return e&&(n="function"==typeof e?e(t):i(i({},t),e)),n},l=function(e){var t=s(e.components);return r.createElement(d.Provider,{value:t},e.children)},p={inlineCode:"code",wrapper:function(e){var t=e.children;return r.createElement(r.Fragment,{},t)}},m=r.forwardRef((function(e,t){var n=e.components,a=e.mdxType,o=e.originalType,d=e.parentName,l=c(e,["components","mdxType","originalType","parentName"]),m=s(n),u=a,f=m["".concat(d,".").concat(u)]||m[u]||p[u]||o;return n?r.createElement(f,i(i({ref:t},l),{},{components:n})):r.createElement(f,i({ref:t},l))}));function u(e,t){var n=arguments,a=t&&t.mdxType;if("string"==typeof e||a){var o=n.length,i=new Array(o);i[0]=m;var c={};for(var d in t)hasOwnProperty.call(t,d)&&(c[d]=t[d]);c.originalType=e,c.mdxType="string"==typeof e?e:a,i[1]=c;for(var s=2;s<o;s++)i[s]=n[s];return r.createElement.apply(null,i)}return r.createElement.apply(null,n)}m.displayName="MDXCreateElement"},3627:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>d,contentTitle:()=>i,default:()=>p,frontMatter:()=>o,metadata:()=>c,toc:()=>s});var r=n(7462),a=(n(7294),n(3905));const o={id:"opcdademo",title:"\u91c7\u96c6OPCDA\u6570\u636e",sidebar_label:"8.5\u3001\u91c7\u96c6OPCDA\u6570\u636e"},i=void 0,c={unversionedId:"08\u3001Demo/opcdademo",id:"08\u3001Demo/opcdademo",title:"\u91c7\u96c6OPCDA\u6570\u636e",description:"\u4e0b\u9762\u6f14\u793a\u7f51\u5173\u63d0\u4f9b\u7684OPCDAClient\u63d2\u4ef6\u4f7f\u7528",source:"@site/docs/08\u3001Demo/8.5\u3001\u91c7\u96c6OPCDA\u6570\u636e.mdx",sourceDirName:"08\u3001Demo",slug:"/08\u3001Demo/opcdademo",permalink:"/thingsgateway/docs/08\u3001Demo/opcdademo",draft:!1,editUrl:"https://gitee.com/diego2098/ThingsGateway/tree/master/handbook/docs/08\u3001Demo/8.5\u3001\u91c7\u96c6OPCDA\u6570\u636e.mdx",tags:[],version:"current",frontMatter:{id:"opcdademo",title:"\u91c7\u96c6OPCDA\u6570\u636e",sidebar_label:"8.5\u3001\u91c7\u96c6OPCDA\u6570\u636e"},sidebar:"tutorialSidebar",previous:{title:"8.4\u3001\u4f7f\u7528MqttClient\u63d2\u4ef6",permalink:"/thingsgateway/docs/08\u3001Demo/mqttclientdemo"},next:{title:"8.6\u3001\u91c7\u96c6OPCUA\u6570\u636e",permalink:"/thingsgateway/docs/08\u3001Demo/opcuademo"}},d={},s=[{value:"\uff08\u4e00\uff09\u5de5\u5177\u51c6\u5907",id:"\u4e00\u5de5\u5177\u51c6\u5907",level:3},{value:"\uff08\u4e8c\uff09\u5efa\u7acb\u91c7\u96c6\u8bbe\u5907",id:"\u4e8c\u5efa\u7acb\u91c7\u96c6\u8bbe\u5907",level:3},{value:"\uff08\u4e09\uff09\u5efa\u7acb\u91c7\u96c6\u53d8\u91cf",id:"\u4e09\u5efa\u7acb\u91c7\u96c6\u53d8\u91cf",level:3},{value:"\uff08\u56db\uff09\u91cd\u542f\u91c7\u96c6\u7ebf\u7a0b",id:"\u56db\u91cd\u542f\u91c7\u96c6\u7ebf\u7a0b",level:3},{value:"\uff08\u4e94\uff09\u67e5\u770b\u72b6\u6001",id:"\u4e94\u67e5\u770b\u72b6\u6001",level:3},{value:"\uff08\u516d\uff09\u5bfc\u5165\u53d8\u91cf",id:"\u516d\u5bfc\u5165\u53d8\u91cf",level:3},{value:"\u6700\u540e\uff0c\u770b\u4e00\u4e0b\u5b9e\u65f6\u53d8\u5316\u6548\u679c",id:"\u6700\u540e\u770b\u4e00\u4e0b\u5b9e\u65f6\u53d8\u5316\u6548\u679c",level:3}],l={toc:s};function p(e){let{components:t,...o}=e;return(0,a.kt)("wrapper",(0,r.Z)({},l,o,{components:t,mdxType:"MDXLayout"}),(0,a.kt)("p",null,"\u4e0b\u9762\u6f14\u793a\u7f51\u5173\u63d0\u4f9b\u7684OPCDAClient\u63d2\u4ef6\u4f7f\u7528"),(0,a.kt)("h3",{id:"\u4e00\u5de5\u5177\u51c6\u5907"},"\uff08\u4e00\uff09\u5de5\u5177\u51c6\u5907"),(0,a.kt)("p",null,"1\u3001OPCDAServer\uff1aKepServer"),(0,a.kt)("h3",{id:"\u4e8c\u5efa\u7acb\u91c7\u96c6\u8bbe\u5907"},"\uff08\u4e8c\uff09\u5efa\u7acb\u91c7\u96c6\u8bbe\u5907"),(0,a.kt)("img",{src:n(2364).Z,width:"400"}),(0,a.kt)("img",{src:n(7095).Z,width:"400"}),(0,a.kt)("p",null," \u8bbe\u5907\u5c5e\u6027\u9ed8\u8ba4"),(0,a.kt)("h3",{id:"\u4e09\u5efa\u7acb\u91c7\u96c6\u53d8\u91cf"},"\uff08\u4e09\uff09\u5efa\u7acb\u91c7\u96c6\u53d8\u91cf"),(0,a.kt)("img",{src:n(3).Z,width:"800"}),(0,a.kt)("p",null,"KepServer\u4e2d\u5efa\u7acb\u4efb\u610f\u6a21\u62df\u8bbe\u5907\uff0c\u5bf9\u5e94ItemId\u586b\u5165\u53d8\u91cf\u5730\u5740\u4e2d\uff0cOPC\u7cfb\u5217\u6570\u636e\u7c7b\u578b\u53ef\u4ee5\u9ed8\u8ba4\u4e3aobject"),(0,a.kt)("h3",{id:"\u56db\u91cd\u542f\u91c7\u96c6\u7ebf\u7a0b"},"\uff08\u56db\uff09\u91cd\u542f\u91c7\u96c6\u7ebf\u7a0b"),(0,a.kt)("p",null,(0,a.kt)("img",{src:n(4235).Z,width:"2320",height:"1417"})),(0,a.kt)("p",null,"\u70b9\u51fb\u53f3\u8fb9\u6d6e\u52a8\u6309\u94ae\uff0c\u5168\u90e8\u91cd\u542f"),(0,a.kt)("h3",{id:"\u4e94\u67e5\u770b\u72b6\u6001"},"\uff08\u4e94\uff09\u67e5\u770b\u72b6\u6001"),(0,a.kt)("img",{src:n(1921).Z,width:"600"}),(0,a.kt)("p",null,"\u70b9\u51fb\u76f8\u5173\u6309\u94ae\uff0c\u53ef\u4ee5\u8df3\u8f6c\u5230\u53d8\u91cf\u5b9e\u65f6\u6570\u636e\u9875\u9762"),(0,a.kt)("p",null,(0,a.kt)("img",{src:n(9628).Z,width:"2288",height:"956"})),(0,a.kt)("h3",{id:"\u516d\u5bfc\u5165\u53d8\u91cf"},"\uff08\u516d\uff09\u5bfc\u5165\u53d8\u91cf"),(0,a.kt)("p",null,"OPC\u63d2\u4ef6\u652f\u6301\u5bfc\u5165OPC\u8282\u70b9"),(0,a.kt)("p",null,(0,a.kt)("img",{src:n(7111).Z,width:"2327",height:"988"}),"\n",(0,a.kt)("img",{src:n(3429).Z,width:"1808",height:"923"}),"\n\u70b9\u51fb\u5bfc\u51faExcel\uff0c\u81ea\u884c\u4fee\u6539\u540e\u5bfc\u5165\u53d8\u91cf\u5373\u53ef"),(0,a.kt)("h3",{id:"\u6700\u540e\u770b\u4e00\u4e0b\u5b9e\u65f6\u53d8\u5316\u6548\u679c"},"\u6700\u540e\uff0c\u770b\u4e00\u4e0b\u5b9e\u65f6\u53d8\u5316\u6548\u679c"),(0,a.kt)("p",null,(0,a.kt)("img",{src:n(9281).Z,width:"2316",height:"1305"})))}p.isMDXComponent=!0},2364:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/opcdademo1-36504859b247ce3f538016128fba4198.png"},7095:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/opcdademo2-b22bf014eb632ba7d4001121fbd94f06.png"},3:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/opcdademo3-167db607d64b0603a5c2c1816bbea675.png"},1921:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/opcdademo4-4e4f2a60bac3781a7c38969fb53af45a.png"},4235:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/modbusdemo4-9e3bec8728b35dda03913ba055cbede7.png"},9628:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/opcdademo5-6c693fb8f25281296aa2c1b9c1819ed6.png"},9281:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/opcdademo6-98fb3dff364d26f9024c81c113388e87.gif"},7111:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/opcdademo7-e609a6463058ff4cfb156b2f6db45d54.png"},3429:(e,t,n)=>{n.d(t,{Z:()=>r});const r=n.p+"assets/images/opcdademo8-648ec498feb1a012f830c6aec2ba72c2.png"}}]);