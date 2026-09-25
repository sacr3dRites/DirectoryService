"use client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useCounter } from "@/hooks/useCounter";
import { useEffect, useState } from "react";
import { JSX } from "react/jsx-runtime";

export default function HelloWorld(): JSX.Element {
  const { counter, handleClick, isWin } = useCounter();

  function calcNumber(a: number, b: number) {
    return a + b;
  }
  return (
    <div className="flex flex-col gap-4">
      <Text text={counter} />
      <Button onClick={handleClick} variant={"destructive"} className={"w-fit"}>
        Increment
      </Button>

      <Input placeholder="Type here..." />

      {isWin && <span>Congrats!</span>}
    </div>
  );
}

function Text({ text }: { text: number }) {
  return <span className="text-red-500">{text}</span>;
}
