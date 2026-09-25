import { useState } from "react";
import { Todo } from "./todo";

export default function useTodos() {
  const [todos, setTodos] = useState<Todo[]>([]);

  const [input, setInput] = useState("");

  const addTodo = () => {
    const text = input.trim();
    if (!text) return;

    const newTodo: Todo = {
      id: crypto.randomUUID(),
      text: text,
      completed: false,
    };
    setTodos([...todos, newTodo]);
    setInput("");
  };

  const toggleTodo = (id: string) => {
    setTodos((prevTodos) =>
      prevTodos.map((todo) =>
        todo.id === id ? { ...todo, completed: !todo.completed } : todo,
      ),
    );
  };

  const deleteTodo = (id: string) => {
    setTodos((prevTodos) => prevTodos.filter((todo) => todo.id !== id));
  };

  const remaining = todos.filter((todo) => !todo.completed).length;

  return { todos, input, setInput, addTodo, toggleTodo, deleteTodo, remaining };
}
