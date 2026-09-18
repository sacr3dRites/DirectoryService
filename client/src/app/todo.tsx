"use client";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@base-ui/react";
import { useState } from "react";

type Todo = {
  id: number;
  text: string;
  completed: boolean;
};

export default function Todo() {
  const [todos, setTodos] = useState<Todo[]>([
    { id: 1, text: "Learn TypeScript", completed: false },
    { id: 2, text: "Build a Next.js app", completed: true },
  ]);

  const [input, setInput] = useState("");

  const addTodo = () => {
    const newTodo: Todo = {
      id: todos.length + 1,
      text: input,
      completed: false,
    };
    setTodos([...todos, newTodo]);
  };

  const toggleTodo = (id: number) => {
    setTodos((prevTodos) =>
      prevTodos.map((todo) =>
        todo.id === id ? { ...todo, completed: !todo.completed } : todo,
      ),
    );
  };

  return (
    <div className="min-h-screen w-full bg-muted/30 px-4 py-10 sm:px-6">
      <main className="mx-auto flex w-full max-w-3xl flex-col gap-8">
        <header className="space-y-2">
          <p className="text-sm font-medium text-muted-foreground">
            Мой список
          </p>
          <h1 className="text-3xl font-semibold tracking-tight">Задачи</h1>
        </header>

        <Card className="border-primary/10 shadow-sm">
          <CardHeader>
            <CardTitle>Новая задача</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col gap-3 sm:flex-row">
              <Input
                value={input}
                onChange={(e) => setInput(e.target.value)}
                className="min-h-10 flex-1 rounded-md border border-input bg-background px-3 text-sm outline-none transition focus-visible:ring-2 focus-visible:ring-ring"
                placeholder="Например, подготовить отчёт"
              />
              <Button onClick={addTodo} className="min-h-10 sm:px-6">
                Добавить
              </Button>
            </div>
          </CardContent>
        </Card>

        <section className="space-y-4" aria-labelledby="tasks-heading">
          <div className="flex items-center justify-between">
            <h2 id="tasks-heading" className="text-lg font-semibold">
              Все задачи
            </h2>
            <span className="text-sm text-muted-foreground">
              {todos.length} всего
            </span>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            {todos.map((todo) => (
              <Card key={todo.id} className="transition-shadow hover:shadow-md">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between gap-4">
                    <Checkbox
                      checked={todo.completed}
                      onCheckedChange={() => toggleTodo(todo.id)}
                    />
                    <CardTitle>Задача #{todo.id}</CardTitle>
                    <span
                      className="text-lg"
                      aria-label={todo.completed ? "Выполнено" : "Не выполнено"}
                    >
                      {todo.completed ? "✅" : "❌"}
                    </span>
                  </div>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground">{todo.text}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </section>
      </main>
    </div>
  );
}
