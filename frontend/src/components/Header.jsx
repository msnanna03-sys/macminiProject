import { NavLink } from 'react-router-dom'

export default function Header() {
  return (
    <header className="site-header">
      <div className="site-header__brand">
        <NavLink to="/">홈서버 블로그</NavLink>
      </div>
      <nav className="site-header__nav">
        <NavLink to="/" end>
          포스트
        </NavLink>
      </nav>
    </header>
  )
}
