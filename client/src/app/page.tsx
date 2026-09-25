import { routes } from "@/config/routes";
import Link from "next/link";

export default function HomePage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-6">
      <h1 className="text-2xl font-bold">Directory Service</h1>
      <nav className="flex flex-col gap-4">
        <Link href={routes.locations}>Локации</Link>
        <Link href={routes.departments}>Подразделения</Link>
        <Link href={routes.positions}>Позиции</Link>
      </nav>
    </div>
  );
}
