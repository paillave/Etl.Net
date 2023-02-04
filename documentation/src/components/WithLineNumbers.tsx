import React from 'react';
import Highlight, { defaultProps } from "prism-react-renderer";
import theme from "prism-react-renderer/themes/dracula";

export function WithLineNumbers({ sourceCode }: { sourceCode: string; }) {
  return (
    <Highlight
      {...defaultProps}
      theme={theme}
      code={sourceCode}
      // @ts-ignore
      language="csharp"
    >
      {({ className, style, tokens, getLineProps, getTokenProps }) => (
        <pre className={className + " language-csharp frontpage"} style={style}>
          {tokens.map((line, i) => (
            <div key={i} {...getLineProps({ line, key: i })}>
              <span>
                {line.map((token, key) => (
                  <span key={key} {...getTokenProps({ token, key })} />
                ))}
              </span>
            </div>
          ))}
        </pre>
      )}
    </Highlight>
  );
} // eslint-disable-line

