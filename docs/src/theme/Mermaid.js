import React, { useEffect } from "react";
import mermaid from "mermaid";

mermaid.initialize({
    startOnLoad: true
});

const Mermaid = ({ chart }) => {
    useEffect(() => {
        mermaid.contentLoaded();
    }, []);
    return <pre className={"prism-code language-csharp codeBlock_node_modules-@docusaurus-theme-classic-lib-next-theme-CodeBlock-styles-module thin-scrollbar"} style={{backgroundColor: "rgb(248, 248, 242)",color: "rgb(40, 42, 54)"}}>
        <div className="mermaid">{chart}</div></pre>;
};

export default Mermaid;