"use strict";(self.webpackChunkthingsgateway=self.webpackChunkthingsgateway||[]).push([[9442],{3905:(t,e,n)=>{n.d(e,{Zo:()=>d,kt:()=>k});var a=n(7294);function r(t,e,n){return e in t?Object.defineProperty(t,e,{value:n,enumerable:!0,configurable:!0,writable:!0}):t[e]=n,t}function l(t,e){var n=Object.keys(t);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(t);e&&(a=a.filter((function(e){return Object.getOwnPropertyDescriptor(t,e).enumerable}))),n.push.apply(n,a)}return n}function i(t){for(var e=1;e<arguments.length;e++){var n=null!=arguments[e]?arguments[e]:{};e%2?l(Object(n),!0).forEach((function(e){r(t,e,n[e])})):Object.getOwnPropertyDescriptors?Object.defineProperties(t,Object.getOwnPropertyDescriptors(n)):l(Object(n)).forEach((function(e){Object.defineProperty(t,e,Object.getOwnPropertyDescriptor(n,e))}))}return t}function p(t,e){if(null==t)return{};var n,a,r=function(t,e){if(null==t)return{};var n,a,r={},l=Object.keys(t);for(a=0;a<l.length;a++)n=l[a],e.indexOf(n)>=0||(r[n]=t[n]);return r}(t,e);if(Object.getOwnPropertySymbols){var l=Object.getOwnPropertySymbols(t);for(a=0;a<l.length;a++)n=l[a],e.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(t,n)&&(r[n]=t[n])}return r}var o=a.createContext({}),u=function(t){var e=a.useContext(o),n=e;return t&&(n="function"==typeof t?t(e):i(i({},e),t)),n},d=function(t){var e=u(t.components);return a.createElement(o.Provider,{value:e},t.children)},c={inlineCode:"code",wrapper:function(t){var e=t.children;return a.createElement(a.Fragment,{},e)}},m=a.forwardRef((function(t,e){var n=t.components,r=t.mdxType,l=t.originalType,o=t.parentName,d=p(t,["components","mdxType","originalType","parentName"]),m=u(n),k=r,g=m["".concat(o,".").concat(k)]||m[k]||c[k]||l;return n?a.createElement(g,i(i({ref:e},d),{},{components:n})):a.createElement(g,i({ref:e},d))}));function k(t,e){var n=arguments,r=e&&e.mdxType;if("string"==typeof t||r){var l=n.length,i=new Array(l);i[0]=m;var p={};for(var o in e)hasOwnProperty.call(e,o)&&(p[o]=e[o]);p.originalType=t,p.mdxType="string"==typeof t?t:r,i[1]=p;for(var u=2;u<l;u++)i[u]=n[u];return a.createElement.apply(null,i)}return a.createElement.apply(null,n)}m.displayName="MDXCreateElement"},2183:(t,e,n)=>{n.r(e),n.d(e,{assets:()=>o,contentTitle:()=>i,default:()=>c,frontMatter:()=>l,metadata:()=>p,toc:()=>u});var a=n(7462),r=(n(7294),n(3905));const l={id:"pluginkafka",title:"Kafka\u6d88\u606f\u751f\u4ea7"},i=void 0,p={unversionedId:"pluginkafka",id:"pluginkafka",title:"Kafka\u6d88\u606f\u751f\u4ea7",description:"\u4e00\u3001\u8bbe\u5907\u6269\u5c55\u5c5e\u6027",source:"@site/docs/pluginkafka.mdx",sourceDirName:".",slug:"/pluginkafka",permalink:"/thingsgateway/docs/pluginkafka",draft:!1,editUrl:"https://gitee.com/dotnetchina/ThingsGateway/tree/master/handbook/docs/pluginkafka.mdx",tags:[],version:"current",frontMatter:{id:"pluginkafka",title:"Kafka\u6d88\u606f\u751f\u4ea7"},sidebar:"docs",previous:{title:"RabbitMQClient",permalink:"/thingsgateway/docs/pluginrabbitmqclient"},next:{title:"\u53d1\u5e03",permalink:"/thingsgateway/docs/release"}},o={},u=[{value:"\u4e00\u3001\u8bbe\u5907\u6269\u5c55\u5c5e\u6027",id:"\u4e00\u8bbe\u5907\u6269\u5c55\u5c5e\u6027",level:2},{value:"\u4e8c\u3001\u53d8\u91cf\u914d\u7f6e",id:"\u4e8c\u53d8\u91cf\u914d\u7f6e",level:2},{value:"\u542f\u7528",id:"\u542f\u7528",level:3}],d={toc:u};function c(t){let{components:e,...l}=t;return(0,r.kt)("wrapper",(0,a.Z)({},d,l,{components:e,mdxType:"MDXLayout"}),(0,r.kt)("h2",{id:"\u4e00\u8bbe\u5907\u6269\u5c55\u5c5e\u6027"},"\u4e00\u3001\u8bbe\u5907\u6269\u5c55\u5c5e\u6027"),(0,r.kt)("img",{src:n(4945).Z,width:"300"}),(0,r.kt)("table",null,(0,r.kt)("thead",{parentName:"table"},(0,r.kt)("tr",{parentName:"thead"},(0,r.kt)("th",{parentName:"tr",align:null},"\u5c5e\u6027"),(0,r.kt)("th",{parentName:"tr",align:null},"\u8bf4\u660e"),(0,r.kt)("th",{parentName:"tr",align:null},"\u9ed8\u8ba4\u503c/\u5907\u6ce8"))),(0,r.kt)("tbody",{parentName:"table"},(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"\u670d\u52a1\u5730\u5740"),(0,r.kt)("td",{parentName:"tr",align:null},"\u670d\u52a1\u5730\u5740"),(0,r.kt)("td",{parentName:"tr",align:null},"127.0.0.1:9092")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"\u8bbe\u5907\u4e3b\u9898"),(0,r.kt)("td",{parentName:"tr",align:null},"\u8bbe\u5907\u4e3b\u9898"),(0,r.kt)("td",{parentName:"tr",align:null})),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"\u53d8\u91cf\u4e3b\u9898"),(0,r.kt)("td",{parentName:"tr",align:null},"\u53d8\u91cf\u4e3b\u9898"),(0,r.kt)("td",{parentName:"tr",align:null})),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"\u5ba2\u6237\u7aefID"),(0,r.kt)("td",{parentName:"tr",align:null},"\u5ba2\u6237\u7aefID"),(0,r.kt)("td",{parentName:"tr",align:null},"test-consumer")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"\u7ebf\u7a0b\u5faa\u73af\u95f4\u9694"),(0,r.kt)("td",{parentName:"tr",align:null},"\u4e0a\u4f20\u7ebf\u7a0b\u7684\u5faa\u73af\u95f4\u9694,\u4e00\u822c\u4e0d\u9700\u8981\u66f4\u6539(ms)"),(0,r.kt)("td",{parentName:"tr",align:null},"1000")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"\u53d1\u5e03\u8d85\u65f6\u65f6\u95f4"),(0,r.kt)("td",{parentName:"tr",align:null},"\u8fde\u63a5\u53d1\u5e03\u65f6\u7684\u8d85\u65f6\u65f6\u95f4"),(0,r.kt)("td",{parentName:"tr",align:null},"1000")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"\u7f13\u5b58\u6700\u5927\u6761\u6570"),(0,r.kt)("td",{parentName:"tr",align:null},"\u79bb\u7ebf\u7f13\u5b58\u65f6\u7684\u6700\u5927\u6761\u6570,\u8fd9\u91cc\u6307\u7684\u662f\u53d1\u5e03\u4e0d\u6210\u529f\u65f6\u7684\u7f13\u5b58\u6b21\u6570"),(0,r.kt)("td",{parentName:"tr",align:null},"2000")),(0,r.kt)("tr",{parentName:"tbody"},(0,r.kt)("td",{parentName:"tr",align:null},"\u5217\u8868\u5206\u5272\u5927\u5c0f"),(0,r.kt)("td",{parentName:"tr",align:null},"\u53d1\u5e03\u7684\u5b9e\u4f53\u5217\u8868\u6309\u5927\u5c0f\u5206\u5272"),(0,r.kt)("td",{parentName:"tr",align:null},"1000")))),(0,r.kt)("h2",{id:"\u4e8c\u53d8\u91cf\u914d\u7f6e"},"\u4e8c\u3001\u53d8\u91cf\u914d\u7f6e"),(0,r.kt)("img",{src:n(2746).Z,width:"300"}),(0,r.kt)("h3",{id:"\u542f\u7528"},"\u542f\u7528"),(0,r.kt)("p",null,"\u542f\u7528\u53d8\u91cf\u540e\u624d\u80fd\u8fdb\u884c\u4e0a\u4f20"))}c.isMDXComponent=!0},4945:(t,e,n)=>{n.d(e,{Z:()=>a});const a=n.p+"assets/images/pluginkafka-1-13ee5e076f47d5019df8a5f7f5211188.png"},2746:(t,e,n)=>{n.d(e,{Z:()=>a});const a=n.p+"assets/images/pluginkafka-2-24d83aaf5bbe312cf702b16491d9b03c.png"}}]);