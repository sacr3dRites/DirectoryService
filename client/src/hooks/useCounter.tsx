import { useState } from "react";

export function useCounter() {
  const [counter, setCounter] = useState(0);

  const click = () => {
    setCounter((prevCounter) => prevCounter + 1);
  };

  const isWin = counter > 30;

  return { counter, handleClick: click, isWin };
}
