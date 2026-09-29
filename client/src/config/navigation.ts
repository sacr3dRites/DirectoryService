import { Building, MapPin, User } from "lucide-react";
import { routes } from "./routes";

export const menuItems = [
  { label: "Локации", href: routes.locations, icon: MapPin },
  { label: "Подразделения", href: routes.departments, icon: Building },
  { label: "Позиции", href: routes.positions, icon: User },
];
