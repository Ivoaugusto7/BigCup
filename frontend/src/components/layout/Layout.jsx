function Layout({ header, children }) {
  return (
    <div className="flex min-h-screen flex-col bg-background text-slate-100">
      {header}
      <main className="mx-auto flex w-full max-w-6xl flex-1 items-center justify-center px-6 py-16 sm:px-8 lg:px-10">
        {children}
      </main>
    </div>
  );
}

export default Layout;
