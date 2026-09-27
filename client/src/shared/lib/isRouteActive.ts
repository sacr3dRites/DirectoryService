export default function isRouteActive(route: string, href: string) {
  if (href === "/") {
    return route === href;
  }

  return route === href || route.startsWith(`${href}/`);
}
