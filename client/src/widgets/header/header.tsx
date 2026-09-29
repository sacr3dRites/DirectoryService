import { routes } from "@/config/routes";
import Link from "next/link";
import { SidebarTrigger } from "../../shared/components/ui/sidebar";

export default function Header() {
  return (
    <header className="sticky top-0 z-50 bg-background text-primary-foreground px-3">
      <div className="container flex h-16 items-center gap-4">
        <SidebarTrigger
          className="md:hidden bg-red-50"
          aria-label="Переключить меню"
        />
        <div className="h-8 w-8 rounded-full bg-red-50 flex items-center gap-2 justify-center text-sm font-bold text-red-500 ring-1 ring-inset ring-red-500/10">
          DS
        </div>
        <Link
          href={routes.home}
          className="flex items-center gap-2 text-primary"
        >
          <span>Directory Service</span>
        </Link>
      </div>
    </header>
  );
}
