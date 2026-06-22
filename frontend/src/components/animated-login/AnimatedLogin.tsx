import { motion } from "framer-motion";
import { Eye, EyeOff, Sparkles } from "lucide-react";
import {
	type CSSProperties,
	type FormEvent,
	type RefObject,
	useEffect,
	useRef,
	useState,
} from "react";
import Button from "react-bootstrap/Button";
import Container from "react-bootstrap/Container";
import Form from "react-bootstrap/Form";
import { useTranslations } from "../../context/TranslationContext";

const charTransition = { duration: 0.7, ease: "easeInOut" as const };

interface PupilProps {
	size?: number;
	maxDistance?: number;
	pupilColor?: string;
	forceLookX?: number;
	forceLookY?: number;
}

const Pupil = ({
	size = 12,
	maxDistance = 5,
	pupilColor = "black",
	forceLookX,
	forceLookY,
}: PupilProps) => {
	const [mouseX, setMouseX] = useState<number>(0);
	const [mouseY, setMouseY] = useState<number>(0);
	const pupilRef = useRef<HTMLDivElement>(null);

	useEffect(() => {
		const handleMouseMove = (e: MouseEvent) => {
			setMouseX(e.clientX);
			setMouseY(e.clientY);
		};

		window.addEventListener("mousemove", handleMouseMove);

		return () => {
			window.removeEventListener("mousemove", handleMouseMove);
		};
	}, []);

	const calculatePupilPosition = () => {
		if (!pupilRef.current) return { x: 0, y: 0 };

		if (forceLookX !== undefined && forceLookY !== undefined) {
			return { x: forceLookX, y: forceLookY };
		}

		const pupil = pupilRef.current.getBoundingClientRect();
		const pupilCenterX = pupil.left + pupil.width / 2;
		const pupilCenterY = pupil.top + pupil.height / 2;

		const deltaX = mouseX - pupilCenterX;
		const deltaY = mouseY - pupilCenterY;
		const distance = Math.min(
			Math.sqrt(deltaX ** 2 + deltaY ** 2),
			maxDistance,
		);

		const angle = Math.atan2(deltaY, deltaX);
		const x = Math.cos(angle) * distance;
		const y = Math.sin(angle) * distance;

		return { x, y };
	};

	const pupilPosition = calculatePupilPosition();

	return (
		<div
			ref={pupilRef}
			className="rounded-circle"
			style={{
				width: `${size}px`,
				height: `${size}px`,
				backgroundColor: pupilColor,
				transform: `translate(${pupilPosition.x}px, ${pupilPosition.y}px)`,
				transition: "transform 0.1s ease-out",
			}}
		/>
	);
};

interface EyeBallProps {
	size?: number;
	pupilSize?: number;
	maxDistance?: number;
	eyeColor?: string;
	pupilColor?: string;
	isBlinking?: boolean;
	forceLookX?: number;
	forceLookY?: number;
}

const EyeBall = ({
	size = 48,
	pupilSize = 16,
	maxDistance = 10,
	eyeColor = "white",
	pupilColor = "black",
	isBlinking = false,
	forceLookX,
	forceLookY,
}: EyeBallProps) => {
	const [mouseX, setMouseX] = useState<number>(0);
	const [mouseY, setMouseY] = useState<number>(0);
	const eyeRef = useRef<HTMLDivElement>(null);

	useEffect(() => {
		const handleMouseMove = (e: MouseEvent) => {
			setMouseX(e.clientX);
			setMouseY(e.clientY);
		};

		window.addEventListener("mousemove", handleMouseMove);

		return () => {
			window.removeEventListener("mousemove", handleMouseMove);
		};
	}, []);

	const calculatePupilPosition = () => {
		if (!eyeRef.current) return { x: 0, y: 0 };

		if (forceLookX !== undefined && forceLookY !== undefined) {
			return { x: forceLookX, y: forceLookY };
		}

		const eye = eyeRef.current.getBoundingClientRect();
		const eyeCenterX = eye.left + eye.width / 2;
		const eyeCenterY = eye.top + eye.height / 2;

		const deltaX = mouseX - eyeCenterX;
		const deltaY = mouseY - eyeCenterY;
		const distance = Math.min(
			Math.sqrt(deltaX ** 2 + deltaY ** 2),
			maxDistance,
		);

		const angle = Math.atan2(deltaY, deltaX);
		const x = Math.cos(angle) * distance;
		const y = Math.sin(angle) * distance;

		return { x, y };
	};

	const pupilPosition = calculatePupilPosition();

	return (
		<div
			ref={eyeRef}
			className="rounded-circle d-flex align-items-center justify-content-center"
			style={{
				width: `${size}px`,
				height: isBlinking ? "2px" : `${size}px`,
				backgroundColor: eyeColor,
				overflow: "hidden",
				transition: "all 150ms ease",
			}}
		>
			{!isBlinking && (
				<div
					className="rounded-circle"
					style={{
						width: `${pupilSize}px`,
						height: `${pupilSize}px`,
						backgroundColor: pupilColor,
						transform: `translate(${pupilPosition.x}px, ${pupilPosition.y}px)`,
						transition: "transform 0.1s ease-out",
					}}
				/>
			)}
		</div>
	);
};

export type AnimatedLoginProps = {
	email: string;
	password: string;
	onEmailChange: (value: string) => void;
	onPasswordChange: (value: string) => void;
	onLoginRequest: (email: string, password: string) => void | Promise<void>;
	externalError?: string;
};

export default function AnimatedLogin({
	email,
	password,
	onEmailChange,
	onPasswordChange,
	onLoginRequest,
	externalError = "",
}: AnimatedLoginProps) {
	const [showPassword, setShowPassword] = useState(false);
	const [isLoading, setIsLoading] = useState(false);
	const [mouseX, setMouseX] = useState<number>(0);
	const [mouseY, setMouseY] = useState<number>(0);
	const [isPurpleBlinking, setIsPurpleBlinking] = useState(false);
	const [isBlackBlinking, setIsBlackBlinking] = useState(false);
	const [isTyping, setIsTyping] = useState(false);
	const [isLookingAtEachOther, setIsLookingAtEachOther] = useState(false);
	const [isPurplePeeking, setIsPurplePeeking] = useState(false);
	const purpleRef = useRef<HTMLDivElement>(null);
	const blackRef = useRef<HTMLDivElement>(null);
	const yellowRef = useRef<HTMLDivElement>(null);
	const orangeRef = useRef<HTMLDivElement>(null);

	useEffect(() => {
		const handleMouseMove = (e: MouseEvent) => {
			setMouseX(e.clientX);
			setMouseY(e.clientY);
		};

		window.addEventListener("mousemove", handleMouseMove);
		return () => window.removeEventListener("mousemove", handleMouseMove);
	}, []);

	useEffect(() => {
		const getRandomBlinkInterval = () => Math.random() * 4000 + 3000;

		const scheduleBlink = () => {
			const blinkTimeout = setTimeout(() => {
				setIsPurpleBlinking(true);
				setTimeout(() => {
					setIsPurpleBlinking(false);
					scheduleBlink();
				}, 150);
			}, getRandomBlinkInterval());

			return blinkTimeout;
		};

		const timeout = scheduleBlink();
		return () => clearTimeout(timeout);
	}, []);

	useEffect(() => {
		const getRandomBlinkInterval = () => Math.random() * 4000 + 3000;

		const scheduleBlink = () => {
			const blinkTimeout = setTimeout(() => {
				setIsBlackBlinking(true);
				setTimeout(() => {
					setIsBlackBlinking(false);
					scheduleBlink();
				}, 150);
			}, getRandomBlinkInterval());

			return blinkTimeout;
		};

		const timeout = scheduleBlink();
		return () => clearTimeout(timeout);
	}, []);

	useEffect(() => {
		if (isTyping) {
			setIsLookingAtEachOther(true);
			const timer = setTimeout(() => {
				setIsLookingAtEachOther(false);
			}, 800);
			return () => clearTimeout(timer);
		} else {
			setIsLookingAtEachOther(false);
		}
	}, [isTyping]);

	useEffect(() => {
		if (externalError) {
			setIsPurpleBlinking(true);
			setIsBlackBlinking(true);

			const cringeTimer = setTimeout(() => {
				setIsPurpleBlinking(false);
				setIsBlackBlinking(false);
			}, 1000);

			return () => clearTimeout(cringeTimer);
		}
	}, [externalError]);

	useEffect(() => {
		if (password.length > 0 && showPassword) {
			const schedulePeek = () => {
				const peekInterval = setTimeout(
					() => {
						setIsPurplePeeking(true);
						setTimeout(() => {
							setIsPurplePeeking(false);
						}, 800);
					},
					Math.random() * 3000 + 2000,
				);
				return peekInterval;
			};

			const firstPeek = schedulePeek();
			return () => clearTimeout(firstPeek);
		} else {
			setIsPurplePeeking(false);
		}
	}, [password, showPassword]);

	const calculatePosition = (ref: RefObject<HTMLDivElement | null>) => {
		if (!ref.current) return { faceX: 0, faceY: 0, bodySkew: 0 };

		const rect = ref.current.getBoundingClientRect();
		const centerX = rect.left + rect.width / 2;
		const centerY = rect.top + rect.height / 3;

		const deltaX = mouseX - centerX;
		const deltaY = mouseY - centerY;

		const faceX = Math.max(-15, Math.min(15, deltaX / 20));
		const faceY = Math.max(-10, Math.min(10, deltaY / 30));

		const bodySkew = Math.max(-6, Math.min(6, -deltaX / 120));

		return { faceX, faceY, bodySkew };
	};

	const purplePos = calculatePosition(purpleRef);
	const blackPos = calculatePosition(blackRef);
	const yellowPos = calculatePosition(yellowRef);
	const orangePos = calculatePosition(orangeRef);

	const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
		e.preventDefault();
		setIsLoading(true);
		try {
			await onLoginRequest(email, password);
		} finally {
			setIsLoading(false);
		}
	};

	const leftPanelStyle: CSSProperties = {
		background:
			"linear-gradient(to bottom right, rgba(var(--bs-primary-rgb), 0.9), var(--bs-primary), rgba(var(--bs-primary-rgb), 0.85))",
	};

	const { t } = useTranslations();

	return (
		<div className="min-vh-100 w-100 row g-0 mx-0">
			<div
				className="col-lg-6 position-relative d-none d-lg-flex flex-column justify-content-between p-5 text-white overflow-hidden"
				style={leftPanelStyle}
			>
				<div className="position-relative" style={{ zIndex: 20 }}>
					<div className="d-flex align-items-center gap-2 fs-5 fw-semibold">
						<div
							className="d-flex align-items-center justify-content-center rounded-3 bg-white bg-opacity-10"
							style={{
								width: "2rem",
								height: "2rem",
								backdropFilter: "blur(4px)",
							}}
						>
							<Sparkles size={16} />
						</div>
						<span>Crystal</span>
					</div>
				</div>

				<div
					className="position-relative d-flex align-items-end justify-content-center"
					style={{ zIndex: 20, height: "500px" }}
				>
					<div
						className="position-relative"
						style={{ width: "550px", height: "400px" }}
					>
						<motion.div
							ref={purpleRef}
							className="position-absolute bottom-0"
							transition={charTransition}
							style={{
								left: "70px",
								width: "180px",
								height:
									isTyping || (password.length > 0 && !showPassword)
										? "440px"
										: "400px",
								backgroundColor: "#6C3FF5",
								borderRadius: "10px 10px 0 0",
								zIndex: 1,
								transform:
									password.length > 0 && showPassword
										? `skewX(0deg)`
										: isTyping || (password.length > 0 && !showPassword)
											? `skewX(${(purplePos.bodySkew || 0) - 12}deg) translateX(40px)`
											: `skewX(${purplePos.bodySkew || 0}deg)`,
								transformOrigin: "bottom center",
							}}
						>
							<div
								className="position-absolute d-flex gap-4"
								style={{
									transition: "left 0.7s ease-in-out, top 0.7s ease-in-out",
									left:
										password.length > 0 && showPassword
											? `${20}px`
											: isLookingAtEachOther
												? `${55}px`
												: `${45 + purplePos.faceX}px`,
									top:
										password.length > 0 && showPassword
											? `${35}px`
											: isLookingAtEachOther
												? `${65}px`
												: `${40 + purplePos.faceY}px`,
								}}
							>
								<EyeBall
									size={18}
									pupilSize={7}
									maxDistance={5}
									eyeColor="white"
									pupilColor="#2D2D2D"
									isBlinking={isPurpleBlinking}
									forceLookX={
										password.length > 0 && showPassword
											? isPurplePeeking
												? 4
												: -4
											: isLookingAtEachOther
												? 3
												: undefined
									}
									forceLookY={
										password.length > 0 && showPassword
											? isPurplePeeking
												? 5
												: -4
											: isLookingAtEachOther
												? 4
												: undefined
									}
								/>
								<EyeBall
									size={18}
									pupilSize={7}
									maxDistance={5}
									eyeColor="white"
									pupilColor="#2D2D2D"
									isBlinking={isPurpleBlinking}
									forceLookX={
										password.length > 0 && showPassword
											? isPurplePeeking
												? 4
												: -4
											: isLookingAtEachOther
												? 3
												: undefined
									}
									forceLookY={
										password.length > 0 && showPassword
											? isPurplePeeking
												? 5
												: -4
											: isLookingAtEachOther
												? 4
												: undefined
									}
								/>
							</div>
						</motion.div>

						<motion.div
							ref={blackRef}
							className="position-absolute bottom-0"
							transition={charTransition}
							style={{
								left: "240px",
								width: "120px",
								height: "310px",
								backgroundColor: "#2D2D2D",
								borderRadius: "8px 8px 0 0",
								zIndex: 2,
								transform:
									password.length > 0 && showPassword
										? `skewX(0deg)`
										: isLookingAtEachOther
											? `skewX(${(blackPos.bodySkew || 0) * 1.5 + 10}deg) translateX(20px)`
											: isTyping || (password.length > 0 && !showPassword)
												? `skewX(${(blackPos.bodySkew || 0) * 1.5}deg)`
												: `skewX(${blackPos.bodySkew || 0}deg)`,
								transformOrigin: "bottom center",
							}}
						>
							<div
								className="position-absolute d-flex gap-4"
								style={{
									transition: "left 0.7s ease-in-out, top 0.7s ease-in-out",
									left:
										password.length > 0 && showPassword
											? `${10}px`
											: isLookingAtEachOther
												? `${32}px`
												: `${26 + blackPos.faceX}px`,
									top:
										password.length > 0 && showPassword
											? `${28}px`
											: isLookingAtEachOther
												? `${12}px`
												: `${32 + blackPos.faceY}px`,
								}}
							>
								<EyeBall
									size={16}
									pupilSize={6}
									maxDistance={4}
									eyeColor="white"
									pupilColor="#2D2D2D"
									isBlinking={isBlackBlinking}
									forceLookX={
										password.length > 0 && showPassword
											? -4
											: isLookingAtEachOther
												? 0
												: undefined
									}
									forceLookY={
										password.length > 0 && showPassword
											? -4
											: isLookingAtEachOther
												? -4
												: undefined
									}
								/>
								<EyeBall
									size={16}
									pupilSize={6}
									maxDistance={4}
									eyeColor="white"
									pupilColor="#2D2D2D"
									isBlinking={isBlackBlinking}
									forceLookX={
										password.length > 0 && showPassword
											? -4
											: isLookingAtEachOther
												? 0
												: undefined
									}
									forceLookY={
										password.length > 0 && showPassword
											? -4
											: isLookingAtEachOther
												? -4
												: undefined
									}
								/>
							</div>
						</motion.div>

						<motion.div
							ref={orangeRef}
							className="position-absolute bottom-0"
							transition={charTransition}
							style={{
								left: "0px",
								width: "240px",
								height: "200px",
								zIndex: 3,
								backgroundColor: "#FF9B6B",
								borderRadius: "120px 120px 0 0",
								transform:
									password.length > 0 && showPassword
										? `skewX(0deg)`
										: `skewX(${orangePos.bodySkew || 0}deg)`,
								transformOrigin: "bottom center",
							}}
						>
							<div
								className="position-absolute d-flex gap-4"
								style={{
									transition: "left 200ms ease-out, top 200ms ease-out",
									left:
										password.length > 0 && showPassword
											? `${50}px`
											: `${82 + (orangePos.faceX || 0)}px`,
									top:
										password.length > 0 && showPassword
											? `${85}px`
											: `${90 + (orangePos.faceY || 0)}px`,
								}}
							>
								<Pupil
									size={12}
									maxDistance={5}
									pupilColor="#2D2D2D"
									forceLookX={
										password.length > 0 && showPassword ? -5 : undefined
									}
									forceLookY={
										password.length > 0 && showPassword ? -4 : undefined
									}
								/>
								<Pupil
									size={12}
									maxDistance={5}
									pupilColor="#2D2D2D"
									forceLookX={
										password.length > 0 && showPassword ? -5 : undefined
									}
									forceLookY={
										password.length > 0 && showPassword ? -4 : undefined
									}
								/>
							</div>
						</motion.div>

						<motion.div
							ref={yellowRef}
							className="position-absolute bottom-0"
							transition={charTransition}
							style={{
								left: "310px",
								width: "140px",
								height: "230px",
								backgroundColor: "#E8D754",
								borderRadius: "70px 70px 0 0",
								zIndex: 4,
								transform:
									password.length > 0 && showPassword
										? `skewX(0deg)`
										: `skewX(${yellowPos.bodySkew || 0}deg)`,
								transformOrigin: "bottom center",
							}}
						>
							<div
								className="position-absolute d-flex gap-4"
								style={{
									transition: "left 200ms ease-out, top 200ms ease-out",
									left:
										password.length > 0 && showPassword
											? `${20}px`
											: `${52 + (yellowPos.faceX || 0)}px`,
									top:
										password.length > 0 && showPassword
											? `${35}px`
											: `${40 + (yellowPos.faceY || 0)}px`,
								}}
							>
								<Pupil
									size={12}
									maxDistance={5}
									pupilColor="#2D2D2D"
									forceLookX={
										password.length > 0 && showPassword ? -5 : undefined
									}
									forceLookY={
										password.length > 0 && showPassword ? -4 : undefined
									}
								/>
								<Pupil
									size={12}
									maxDistance={5}
									pupilColor="#2D2D2D"
									forceLookX={
										password.length > 0 && showPassword ? -5 : undefined
									}
									forceLookY={
										password.length > 0 && showPassword ? -4 : undefined
									}
								/>
							</div>
							<div
								className="position-absolute rounded-pill bg-dark"
								style={{
									width: "5rem",
									height: "4px",
									transition: "left 200ms ease-out, top 200ms ease-out",
									left:
										password.length > 0 && showPassword
											? `${10}px`
											: `${40 + (yellowPos.faceX || 0)}px`,
									top:
										password.length > 0 && showPassword
											? `${88}px`
											: `${88 + (yellowPos.faceY || 0)}px`,
								}}
							/>
						</motion.div>
					</div>
				</div>

				<div
					className="position-relative d-flex align-items-center gap-4 small text-white text-opacity-75"
					style={{ zIndex: 20 }}
				>
					<a
						href="https://dev.azure.com/csf-dfc/ERP%20simplifi%C3%A9"
						className="text-white text-opacity-75 text-decoration-none link-light"
					>
						{t.auth.contact}
					</a>
				</div>

				<div
					className="position-absolute top-0 start-0 w-100 h-100"
					style={{
						pointerEvents: "none",
						backgroundImage:
							"linear-gradient(rgba(255,255,255,0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.05) 1px, transparent 1px)",
						backgroundSize: "20px 20px",
					}}
				/>
				<div
					className="position-absolute rounded-circle bg-white bg-opacity-10 opacity-75"
					style={{
						top: "25%",
						right: "25%",
						width: "16rem",
						height: "16rem",
						filter: "blur(48px)",
					}}
				/>
				<div
					className="position-absolute rounded-circle bg-white bg-opacity-10 opacity-50"
					style={{
						bottom: "25%",
						left: "25%",
						width: "24rem",
						height: "24rem",
						filter: "blur(48px)",
					}}
				/>
			</div>

			<div className="col-12 col-lg-6 d-flex align-items-center justify-content-center p-4 p-lg-5 bg-body">
				<Container className="px-0" style={{ maxWidth: "420px" }}>
					<div className="d-flex d-lg-none align-items-center justify-content-center gap-2 fs-5 fw-semibold mb-5">
						<div
							className="d-flex align-items-center justify-content-center rounded-3 bg-primary bg-opacity-10"
							style={{ width: "2rem", height: "2rem" }}
						>
							<Sparkles size={16} className="text-primary" />
						</div>
						<span>Crystal</span>
					</div>

					<div className="text-center mb-4 pb-2 text-dark">
						<h1 className="h2 fw-bold mb-2">{t.auth.welcomeBack}</h1>
						<p className="text-secondary small mb-0">
							{t.auth.enterDetails}
						</p>
					</div>

					<Form onSubmit={handleSubmit} className="d-flex flex-column gap-4">
						<Form.Group controlId="email">
							<Form.Label className="small fw-medium text-dark text-start">{t.auth.email}</Form.Label>
							<Form.Control
								type="email"
								placeholder="anna@gmail.com"
								value={email}
								autoComplete="off"
								onChange={(e) => onEmailChange(e.target.value)}
								onFocus={() => setIsTyping(true)}
								onBlur={() => setIsTyping(false)}
								required
								size="lg"
								className="border-secondary border-opacity-50"
							/>
						</Form.Group>

						<Form.Group controlId="password">
							<Form.Label className="small fw-medium text-dark text-start">{t.auth.password}</Form.Label>
							<div className="position-relative">
								<Form.Control
									type={showPassword ? "text" : "password"}
									placeholder="••••••••"
									value={password}
									onChange={(e) => onPasswordChange(e.target.value)}
									required
									size="lg"
									className="pe-5 border-secondary border-opacity-50"
								/>
								<button
									type="button"
									onClick={() => setShowPassword(!showPassword)}
									className="btn btn-link position-absolute top-50 end-0 translate-middle-y text-secondary p-2 me-1 border-0 shadow-none"
									aria-label={
										showPassword
											? "Masquer le mot de passe"
											: "Afficher le mot de passe"
									}
								>
									{showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
								</button>
							</div>
						</Form.Group>

						<div className="d-flex align-items-center justify-content-between flex-wrap gap-2">
							<Form.Check
								type="checkbox"
								id="remember"
								label={<span className="small text-dark user-select-none">{t.auth.thirtyDays}</span>}
							/>

							<button
								type="button"
								className="btn btn-link p-0 border-0 bg-transparent small text-primary fw-medium text-decoration-none"
							>
								{t.auth.forgotPassword}
							</button>
						</div>

						{externalError && (
							<div className="alert alert-danger py-2 small mb-0" role="alert">
								{externalError}
							</div>
						)}

						<Button
							type="submit"
							size="lg"
							className="w-100 fw-medium"
							disabled={isLoading}
							variant="primary"
						>
							{isLoading ? "Signing in..." : "Log in"}
						</Button>
					</Form>

					<p className="text-center text-secondary small mt-4 mb-0">
						{t.auth.noAccount}
						<button
							type="button"
							className="btn btn-link p-0 border-0 bg-transparent text-body fw-medium text-decoration-none"
						>
							{t.auth.signUp}
						</button>
					</p>
				</Container>
			</div>
		</div>
	);
}
